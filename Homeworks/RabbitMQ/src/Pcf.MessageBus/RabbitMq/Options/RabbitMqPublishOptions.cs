namespace Pcf.MessageBus.RabbitMq.Options;

public sealed class RabbitMqPublishOptions
{
    public string Exchange { get; set; } = default!;
    public string ExchangeType { get; set; } = "topic";
    public string RoutingKey { get; set; } = default!;
    public bool Durable { get; set; } = true;
    public bool Mandatory { get; set; } = true;
}