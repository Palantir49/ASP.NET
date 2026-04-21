namespace Pcf.MessageBus.RabbitMq.Options;

public sealed class RabbitMqConnectionOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ClientProvidedName { get; set; } = "taskorium-messagebus";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public bool TopologyRecoveryEnabled { get; set; } = true;
    public ushort ConsumerDispatchConcurrency { get; set; } = 1;
}