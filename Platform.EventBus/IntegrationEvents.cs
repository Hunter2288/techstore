namespace Platform.EventBus;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public int SchemaVersion { get; init; } = 1;
}

public interface IProductSnapshotEvent
{
    long ProductId { get; }
    string ProductName { get; }
    decimal Price { get; }
    bool IsActive { get; }
}

public sealed record ProductCreatedEvent(long ProductId, string ProductName, decimal Price, bool IsActive = true)
    : IntegrationEvent, IProductSnapshotEvent;

public sealed record ProductPriceChangedEvent(long ProductId, string ProductName, decimal OldPrice, decimal NewPrice,
    bool IsActive = true) : IntegrationEvent, IProductSnapshotEvent
{
    public decimal Price => NewPrice;
}

public sealed record ProductRenamedEvent(long ProductId, string OldName, string ProductName, decimal Price,
    bool IsActive = true) : IntegrationEvent, IProductSnapshotEvent;

public sealed record ProductDeletedEvent(long ProductId, string ProductName, decimal Price, bool IsActive = false)
    : IntegrationEvent, IProductSnapshotEvent;
