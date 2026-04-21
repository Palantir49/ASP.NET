using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Options;

namespace Pcf.MessageBus.RabbitMq.Topology;

public interface IEventTopologyRegistry
{
    RabbitMqPublishOptions GetPublishOptions<TEvent>() where TEvent : IntegrationEvent;
}