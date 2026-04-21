using Microsoft.Extensions.Options;
using Pcf.MessageBus.RabbitMq.Options;

namespace Pcf.MessageBus.RabbitMq.Topology;

public sealed class RabbitMqSubscriptionOptionsProvider(IOptions<RabbitMqOptions> options)
    : IRabbitMqSubscriptionOptionsProvider
{
    private readonly IReadOnlyDictionary<string, RabbitMqSubscriptionOptions> _subscriptions = options.Value.Subscriptions;

    public RabbitMqSubscriptionOptions Get(string key)
    {
        if (_subscriptions.TryGetValue(key, out var value))
            return value;

        throw new InvalidOperationException($"Subscription options with key '{key}' are not configured.");
    }
}