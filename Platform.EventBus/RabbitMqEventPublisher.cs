using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Platform.EventBus;

public sealed class RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger) : IEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task PublishAsync<T>(T integrationEvent, string routingKey, CancellationToken cancellationToken)
        where T : IntegrationEvent
    {
        using var activity = TracePropagation.Source.StartActivity("rabbitmq publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", RabbitMqTopology.CatalogExchange);
        activity?.SetTag("messaging.destination.routing_key", routingKey);
        activity?.SetTag("messaging.message.id", integrationEvent.EventId.ToString());
        activity?.SetTag("event.type", typeof(T).Name);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(true, true, null, null), cancellationToken);
        await RabbitMqTopology.DeclareCatalogConsumerAsync(channel, RabbitMqTopology.OrdersQueue, cancellationToken);
        await RabbitMqTopology.DeclareCatalogConsumerAsync(channel, RabbitMqTopology.ReviewsQueue, cancellationToken);
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = integrationEvent.EventId.ToString(),
            Type = typeof(T).Name,
            Headers = new Dictionary<string, object?>
            {
                ["x-correlation-id"] = Encoding.UTF8.GetBytes(integrationEvent.CorrelationId ?? string.Empty),
                ["x-schema-version"] = integrationEvent.SchemaVersion
            }
        };
        TracePropagation.Inject(properties);
        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, JsonOptions);
        await channel.BasicPublishAsync(RabbitMqTopology.CatalogExchange, routingKey, mandatory: false,
            basicProperties: properties, body: body, cancellationToken: cancellationToken);
        logger.LogInformation("Published {EventType} {EventId} with correlation {CorrelationId} to {Exchange}/{RoutingKey}",
            typeof(T).Name, integrationEvent.EventId, integrationEvent.CorrelationId,
            RabbitMqTopology.CatalogExchange, routingKey);
    }
}
