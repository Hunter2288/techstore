using RabbitMQ.Client;

namespace Platform.EventBus;

public static class RabbitMqTopology
{
    public const string CatalogExchange = "catalog-events";
    public const string DeadLetterExchange = "catalog-events.dlx";
    public const string OrdersQueue = "orders.catalog-sync";
    public const string ReviewsQueue = "reviews.catalog-sync";

    public static async Task DeclareCatalogConsumerAsync(IChannel channel, string queue, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(CatalogExchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: ct);
        var args = new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadLetterExchange };
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: ct);
        await channel.QueueBindAsync(queue, CatalogExchange, "product.*", cancellationToken: ct);
        var dlq = $"{queue}.dlq";
        await channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(dlq, DeadLetterExchange, string.Empty, cancellationToken: ct);
    }
}
