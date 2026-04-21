using Pcf.MessageBus.RabbitMq;

namespace Pcf.MessageBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    void Subscribe<TEvent, THandler>(Action<SubscriptionBuilder> configure)
        where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}