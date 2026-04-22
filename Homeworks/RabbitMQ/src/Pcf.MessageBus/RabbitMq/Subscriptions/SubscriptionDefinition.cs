using Microsoft.Extensions.DependencyInjection;
using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Options;
using Pcf.MessageBus.RabbitMq.Serialization;

namespace Pcf.MessageBus.RabbitMq.Subscriptions;

internal sealed class SubscriptionDefinition<TEvent, THandler>(
    RabbitMqSubscriptionOptions options)
    : ISubscriptionDefinition
    where TEvent : IntegrationEvent
    where THandler : class, IEventHandler<TEvent>
{
    public Type EventType => typeof(TEvent);
    public Type HandlerType => typeof(THandler);

    public string Exchange => options.Exchange;
    public string ExchangeType => options.ExchangeType;
    public string Queue => options.Queue;
    public string RoutingKey => options.RoutingKey;

    public bool Durable => options.Durable;
    public bool Exclusive => options.Exclusive;
    public bool AutoDelete => options.AutoDelete;
    public ushort PrefetchCount => options.PrefetchCount;

    public bool EnableDeadLetter => options.EnableDeadLetter;
    public string? DeadLetterExchange => options.DeadLetterExchange;
    public string? DeadLetterQueue => options.DeadLetterQueue;
    public string? DeadLetterRoutingKey => options.DeadLetterRoutingKey;

    public bool EnableRetry => options.EnableRetry;
    public string? RetryExchange => options.RetryExchange;
    public string? RetryQueue => options.RetryQueue;
    public string? RetryRoutingKey => options.RetryRoutingKey;
    public int RetryTtlMilliseconds => options.RetryTtlMilliseconds;
    public int MaxRetryCount => options.MaxRetryCount;

    public async Task HandleAsync(IServiceProvider serviceProvider, byte[] body, CancellationToken cancellationToken)
    {
        var serializer = serviceProvider.GetRequiredService<IEventSerializer>();
        var handler = serviceProvider.GetRequiredService<THandler>();

        var @event = serializer.Deserialize<TEvent>(body);
        await handler.Handle(@event, cancellationToken);
    }
}