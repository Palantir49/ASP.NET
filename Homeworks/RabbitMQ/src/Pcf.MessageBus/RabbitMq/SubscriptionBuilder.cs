using Pcf.MessageBus.RabbitMq.Options;

namespace Pcf.MessageBus.RabbitMq;

public sealed class SubscriptionBuilder
{
    internal RabbitMqSubscriptionOptions Options { get; } = new();

    public SubscriptionBuilder WithExchange(string exchange, string exchangeType = "topic")
    {
        Options.Exchange = exchange;
        Options.ExchangeType = exchangeType;
        return this;
    }

    public SubscriptionBuilder WithQueue(
        string queue,
        bool durable = true,
        bool exclusive = false,
        bool autoDelete = false)
    {
        Options.Queue = queue;
        Options.Durable = durable;
        Options.Exclusive = exclusive;
        Options.AutoDelete = autoDelete;
        return this;
    }

    public SubscriptionBuilder WithRoutingKey(string routingKey)
    {
        Options.RoutingKey = routingKey;
        return this;
    }

    public SubscriptionBuilder WithPrefetch(ushort prefetchCount)
    {
        Options.PrefetchCount = prefetchCount;
        return this;
    }

    public SubscriptionBuilder WithDeadLetter(
        string deadLetterExchange,
        string deadLetterQueue,
        string deadLetterRoutingKey)
    {
        Options.EnableDeadLetter = true;
        Options.DeadLetterExchange = deadLetterExchange;
        Options.DeadLetterQueue = deadLetterQueue;
        Options.DeadLetterRoutingKey = deadLetterRoutingKey;
        return this;
    }

    public SubscriptionBuilder WithRetry(
        string retryExchange,
        string retryQueue,
        string retryRoutingKey,
        int retryTtlMilliseconds,
        int maxRetryCount)
    {
        Options.EnableRetry = true;
        Options.RetryExchange = retryExchange;
        Options.RetryQueue = retryQueue;
        Options.RetryRoutingKey = retryRoutingKey;
        Options.RetryTtlMilliseconds = retryTtlMilliseconds;
        Options.MaxRetryCount = maxRetryCount;
        return this;
    }
}