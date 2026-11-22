namespace Platform.EventBus;

public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, string routingKey, CancellationToken cancellationToken)
        where T : IntegrationEvent;
}
