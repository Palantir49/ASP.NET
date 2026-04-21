namespace Pcf.MessageBus.RabbitMq.Options;

public sealed class RabbitMqSubscriptionOptions
{
    public string Exchange { get; set; } = default!;
    public string ExchangeType { get; set; } = "topic";
    public string Queue { get; set; } = default!;
    public string RoutingKey { get; set; } = "#";

    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
    public ushort PrefetchCount { get; set; } = 1;

    public bool EnableDeadLetter { get; set; } = true;
    public string? DeadLetterExchange { get; set; }
    public string? DeadLetterQueue { get; set; }
    public string? DeadLetterRoutingKey { get; set; }

    public bool EnableRetry { get; set; } = true;
    public string? RetryExchange { get; set; }
    public string? RetryQueue { get; set; }
    public string? RetryRoutingKey { get; set; }
    public int RetryTtlMilliseconds { get; set; } = 30000;
    public int MaxRetryCount { get; set; } = 3;
}