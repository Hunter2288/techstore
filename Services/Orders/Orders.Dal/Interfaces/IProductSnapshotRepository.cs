namespace Orders.Dal.Interfaces;

public interface IProductSnapshotRepository
{
    Task UpsertAsync(long productId, string name, decimal price, bool isActive, DateTime occurredAt, CancellationToken ct);
}
