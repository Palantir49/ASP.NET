using Pcf.MessageBus.RabbitMq.Subscriptions;

namespace Pcf.MessageBus.RabbitMq.Options;

public interface IRabbitMqSubscriptionDescriptor
{
    ISubscriptionDefinition Create();
}