using Orders.Dal.Interfaces;
using Platform.EventBus;
using System.Text.Json;

namespace Orders.Api.IntegrationEvents;

public sealed class ProductSnapshotHandler(IUnitOfWork unitOfWork, ILogger<ProductSnapshotHandler> logger)
    : IIntegrationEventHandler
{
    public async Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent is not IProductSnapshotEvent snapshot)
            throw new JsonException($"Unsupported event {integrationEvent.GetType().Name}");
        await unitOfWork.BeginTransactionAsync(ct: cancellationToken);
        try
        {
            if (!await unitOfWork.ProcessedEvents.TryAddAsync(integrationEvent.EventId, cancellationToken))
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
                logger.LogInformation("Duplicate {EventType} {EventId} skipped for product {ProductId}",
                    integrationEvent.GetType().Name, integrationEvent.EventId, snapshot.ProductId);
                return;
            }

            await unitOfWork.ProductSnapshots.UpsertAsync(snapshot.ProductId, snapshot.ProductName,
                snapshot.Price, snapshot.IsActive, integrationEvent.OccurredAt, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            logger.LogInformation("Applied {EventType} {EventId} to ProductRefs for product {ProductId}",
                integrationEvent.GetType().Name, integrationEvent.EventId, snapshot.ProductId);
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
