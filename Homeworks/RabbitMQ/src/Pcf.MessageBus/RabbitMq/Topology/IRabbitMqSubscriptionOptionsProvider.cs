using Pcf.MessageBus.RabbitMq.Options;

namespace Pcf.MessageBus.RabbitMq.Topology;

public interface IRabbitMqSubscriptionOptionsProvider
{
    RabbitMqSubscriptionOptions Get(string key);
}