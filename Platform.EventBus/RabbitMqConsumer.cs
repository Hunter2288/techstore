using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platform.EventBus;

public sealed class RabbitMqConsumerOptions(string queueName)
{
    public string QueueName { get; } = queueName;
    public int PrefetchCount { get; init; } = 10;
    public int MaxRetries { get; init; } = 3;
}

public sealed class RabbitMqConsumer(IConnection connection, IServiceScopeFactory scopeFactory,
    RabbitMqConsumerOptions options, ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true, null, null), stoppingToken);
        await RabbitMqTopology.DeclareCatalogConsumerAsync(_channel, options.QueueName, stoppingToken);
        await _channel.BasicQosAsync(0, checked((ushort)options.PrefetchCount), global: false, stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += (_, args) => HandleMessageAsync(args, stoppingToken);
        await _channel.BasicConsumeAsync(options.QueueName, autoAck: false, consumer, stoppingToken);
        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs args, CancellationToken stoppingToken)
    {
        using var activity = TracePropagation.StartConsumerActivity(args, options.QueueName);
        var eventId = args.BasicProperties.MessageId;
        var eventType = args.BasicProperties.Type;
        var correlationId = TracePropagation.ReadHeader(args.BasicProperties.Headers, "x-correlation-id");
        long? productId = null;
        try
        {
            var integrationEvent = IntegrationEventDeserializer.DeserializeCatalogEvent(
                eventType, args.Body.Span, eventId);
            productId = ((IProductSnapshotEvent)integrationEvent).ProductId;
            activity?.SetTag("event.type", integrationEvent.GetType().Name);
            activity?.SetTag("product.id", productId);
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler>()
                .HandleAsync(integrationEvent, stoppingToken);
            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            logger.LogInformation("Acked {EventType} {EventId} for product {ProductId} from {Queue}; correlation {CorrelationId}",
                eventType, eventId, productId, options.QueueName, correlationId);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Permanent event failure {EventId} type {EventType} product {ProductId} from {Queue}; correlation {CorrelationId}; sending to DLQ",
                eventId, eventType, productId, options.QueueName, correlationId);
            await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            await RetryOrDeadLetterAsync(args, exception, productId, correlationId, stoppingToken);
        }
    }

    private async Task RetryOrDeadLetterAsync(BasicDeliverEventArgs args, Exception exception, long? productId,
        string? correlationId, CancellationToken cancellationToken)
    {
        var headers = args.BasicProperties.Headers ?? new Dictionary<string, object?>();
        var retryCount = 0;
        if (headers.TryGetValue("x-retry-count", out var raw))
            int.TryParse(Convert.ToString(raw, System.Globalization.CultureInfo.InvariantCulture), out retryCount);

        if (retryCount >= options.MaxRetries)
        {
            logger.LogError(exception, "Event {EventId} type {EventType} product {ProductId} exhausted {Retries} retries from {Queue}; sending to DLQ, correlation {CorrelationId}",
                args.BasicProperties.MessageId, args.BasicProperties.Type, productId, retryCount, options.QueueName, correlationId);
            await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
            return;
        }

        var retryProperties = new BasicProperties(args.BasicProperties)
        {
            Persistent = true,
            Headers = new Dictionary<string, object?>(headers) { ["x-retry-count"] = retryCount + 1 }
        };
        await _channel!.BasicPublishAsync(args.Exchange, args.RoutingKey, mandatory: false,
            basicProperties: retryProperties, body: args.Body, cancellationToken: cancellationToken);
        await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        logger.LogWarning(exception, "Re-published event {EventId} type {EventType} product {ProductId} retry {Retry}/{MaxRetries} from {Queue}; correlation {CorrelationId}",
            args.BasicProperties.MessageId, args.BasicProperties.Type, productId, retryCount + 1, options.MaxRetries,
            options.QueueName, correlationId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.DisposeAsync();
    }
}
