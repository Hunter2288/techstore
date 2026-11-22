namespace Platform.EventBus;

public interface IIntegrationEventHandler
{
    Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
