namespace Pcf.MessageBus.RabbitMq.Subscriptions;

public interface ISubscriptionDefinition
{
    Type EventType { get; }
    Type HandlerType { get; }

    string Exchange { get; }
    string ExchangeType { get; }
    string Queue { get; }
    string RoutingKey { get; }

    bool Durable { get; }
    bool Exclusive { get; }
    bool AutoDelete { get; }
    ushort PrefetchCount { get; }

    bool EnableDeadLetter { get; }
    string? DeadLetterExchange { get; }
    string? DeadLetterQueue { get; }
    string? DeadLetterRoutingKey { get; }

    bool EnableRetry { get; }
    string? RetryExchange { get; }
    string? RetryQueue { get; }
    string? RetryRoutingKey { get; }
    int RetryTtlMilliseconds { get; }
    int MaxRetryCount { get; }

    Task HandleAsync(IServiceProvider serviceProvider, byte[] body, CancellationToken cancellationToken);
}