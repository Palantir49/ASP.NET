using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Topology;

namespace Pcf.MessageBus.RabbitMq.Hosting;

public abstract class EventBusSubscriptionConfiguratorBase(IRabbitMqSubscriptionOptionsProvider optionsProvider)
    : IEventBusSubscriptionConfigurator
{
    public abstract void Configure(IEventBus eventBus);

    protected void Subscribe<TEvent, THandler>(
        IEventBus eventBus,
        string subscriptionKey)
        where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        var options = optionsProvider.Get(subscriptionKey);

        eventBus.Subscribe<TEvent, THandler>(x => x
            .WithExchange(options.Exchange, options.ExchangeType)
            .WithQueue(options.Queue, options.Durable, options.Exclusive, options.AutoDelete)
            .WithRoutingKey(options.RoutingKey)
            .WithPrefetch(options.PrefetchCount)
            .WithDeadLetter(
                options.DeadLetterExchange ?? $"{options.Queue}.dlx",
                options.DeadLetterQueue ?? $"{options.Queue}.dlq",
                options.DeadLetterRoutingKey ?? $"{options.Queue}.dlq")
            .WithRetry(
                options.RetryExchange ?? $"{options.Queue}.retry.exchange",
                options.RetryQueue ?? $"{options.Queue}.retry",
                options.RetryRoutingKey ?? $"{options.Queue}.retry",
                options.RetryTtlMilliseconds,
                options.MaxRetryCount));
    }
}