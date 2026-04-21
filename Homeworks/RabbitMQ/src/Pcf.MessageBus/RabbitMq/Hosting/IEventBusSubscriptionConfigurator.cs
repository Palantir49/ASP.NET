using Pcf.MessageBus.Abstractions;

namespace Pcf.MessageBus.RabbitMq.Hosting;

public interface IEventBusSubscriptionConfigurator
{
    void Configure(IEventBus eventBus);
}