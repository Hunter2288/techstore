using Platform.EventBus;
using Reviews.Infrastructure.Persistence;

namespace Reviews.WebApi.IntegrationEvents;

public sealed class CatalogIntegrationEventHandler(ProductSnapshotHandler snapshotHandler)
    : IIntegrationEventHandler
{
    public Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        => snapshotHandler.HandleAsync(integrationEvent, cancellationToken);
}
