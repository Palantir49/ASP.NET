using Pcf.MessageBus.Abstractions;

namespace Pcf.MessageBus.Abstractions;

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task Handle(TEvent @event, CancellationToken cancellationToken);
}