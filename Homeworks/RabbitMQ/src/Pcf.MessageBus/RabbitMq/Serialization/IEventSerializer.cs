using Pcf.MessageBus.Abstractions;

namespace Pcf.MessageBus.RabbitMq.Serialization;

public interface IEventSerializer
{
    byte[] Serialize<TEvent>(TEvent @event) where TEvent : IntegrationEvent;
    TEvent Deserialize<TEvent>(byte[] body) where TEvent : IntegrationEvent;
}