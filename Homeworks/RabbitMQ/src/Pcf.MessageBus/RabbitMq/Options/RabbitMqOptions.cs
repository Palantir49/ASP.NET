namespace Pcf.MessageBus.RabbitMq.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "MessageBus:RabbitMq";

    public RabbitMqConnectionOptions Connection { get; set; } = new();
    public Dictionary<string, RabbitMqPublishOptions> Publish { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, RabbitMqSubscriptionOptions> Subscriptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

}