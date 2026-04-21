using Microsoft.Extensions.Options;
using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Options;

namespace Pcf.MessageBus.RabbitMq.Topology;

public sealed class RabbitMqTopologyRegistry(IOptions<RabbitMqOptions> options) : IEventTopologyRegistry
{
    private readonly IReadOnlyDictionary<string, RabbitMqPublishOptions> _publishOptions = options.Value.Publish;

    public RabbitMqPublishOptions GetPublishOptions<TEvent>() where TEvent : IntegrationEvent
    {
        var key = typeof(TEvent).Name;

        if (_publishOptions.TryGetValue(key, out var value))
            return value;

        throw new InvalidOperationException($"Publish options for event '{key}' are not configured.");
    }
}