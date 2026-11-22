using System.Text.Json;

namespace Platform.EventBus;

public static class IntegrationEventDeserializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static IntegrationEvent DeserializeCatalogEvent(string? eventType, ReadOnlySpan<byte> body, string? messageId)
    {
        IntegrationEvent integrationEvent = eventType switch
        {
            nameof(ProductCreatedEvent) => JsonSerializer.Deserialize<ProductCreatedEvent>(body, Options) ?? throw new JsonException("Empty ProductCreatedEvent"),
            nameof(ProductPriceChangedEvent) => JsonSerializer.Deserialize<ProductPriceChangedEvent>(body, Options) ?? throw new JsonException("Empty ProductPriceChangedEvent"),
            nameof(ProductRenamedEvent) => JsonSerializer.Deserialize<ProductRenamedEvent>(body, Options) ?? throw new JsonException("Empty ProductRenamedEvent"),
            nameof(ProductDeletedEvent) => JsonSerializer.Deserialize<ProductDeletedEvent>(body, Options) ?? throw new JsonException("Empty ProductDeletedEvent"),
            _ => throw new JsonException($"Unsupported catalog event type '{eventType}'")
        };

        if (integrationEvent is not IProductSnapshotEvent snapshot || integrationEvent.EventId == Guid.Empty ||
            snapshot.ProductId <= 0 || string.IsNullOrWhiteSpace(snapshot.ProductName) || snapshot.Price < 0 ||
            integrationEvent.OccurredAt == default ||
            !Guid.TryParse(messageId, out var parsedMessageId) || parsedMessageId != integrationEvent.EventId)
            throw new JsonException("Catalog event is missing valid product snapshot data or metadata");

        return integrationEvent;
    }
}
