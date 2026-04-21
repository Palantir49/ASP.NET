using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Options;
using Pcf.MessageBus.RabbitMq.Serialization;
using Pcf.MessageBus.RabbitMq.Subscriptions;
using Pcf.MessageBus.RabbitMq.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pcf.MessageBus.RabbitMq.Infrastructure;

public sealed class RabbitMqEventBus(
    IServiceScopeFactory scopeFactory,
    IEventTopologyRegistry topologyRegistry,
    IEventSerializer serializer,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqEventBus> logger)
    : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;

    private readonly List<ISubscriptionDefinition> _subscriptions = [];
    private readonly List<(IChannel Channel, string ConsumerTag)> _consumerChannels = [];
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _publishChannel;

    public void Subscribe<TEvent, THandler>(Action<SubscriptionBuilder> configure)
        where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        var builder = new SubscriptionBuilder();
        configure(builder);

        NormalizeAndValidate(builder.Options);

        _subscriptions.Add(new SubscriptionDefinition<TEvent, THandler>(
            builder.Options,
            serializer));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _options.Connection.HostName,
            Port = _options.Connection.Port,
            UserName = _options.Connection.UserName,
            Password = _options.Connection.Password,
            VirtualHost = _options.Connection.VirtualHost,
            ClientProvidedName = _options.Connection.ClientProvidedName,
            AutomaticRecoveryEnabled = _options.Connection.AutomaticRecoveryEnabled,
            TopologyRecoveryEnabled = _options.Connection.TopologyRecoveryEnabled,
            ConsumerDispatchConcurrency = _options.Connection.ConsumerDispatchConcurrency
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _publishChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _publishChannel.BasicReturnAsync += async (_, args) =>
        {
            logger.LogWarning(
                "RabbitMQ returned message. Exchange={Exchange}, RoutingKey={RoutingKey}, ReplyCode={ReplyCode}, ReplyText={ReplyText}",
                args.Exchange,
                args.RoutingKey,
                args.ReplyCode,
                args.ReplyText);

            await Task.CompletedTask;
        };

        foreach (var subscription in _subscriptions)
        {
            var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await DeclareTopologyAsync(channel, subscription, cancellationToken);
            await channel.BasicQosAsync(0, subscription.PrefetchCount, false, cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleMessageAsync(subscription, channel, ea, cancellationToken);
            };

            var consumerTag = await channel.BasicConsumeAsync(
                queue: subscription.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _consumerChannels.Add((channel, consumerTag));

            logger.LogInformation(
                "Consumer started. Event={EventType}, Handler={HandlerType}, Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                subscription.EventType.Name,
                subscription.HandlerType.Name,
                subscription.Queue,
                subscription.Exchange,
                subscription.RoutingKey);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (channel, consumerTag) in _consumerChannels)
        {
            try
            {
                await channel.BasicCancelAsync(consumerTag, cancellationToken:cancellationToken);
                await channel.CloseAsync(cancellationToken);
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while stopping RabbitMQ consumer");
            }
        }

        _consumerChannels.Clear();

        if (_publishChannel is not null)
        {
            await _publishChannel.CloseAsync(cancellationToken);
            await _publishChannel.DisposeAsync();
            _publishChannel = null;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (_publishChannel is null)
            throw new InvalidOperationException("Event bus is not started.");

        var options = topologyRegistry.GetPublishOptions<TEvent>();

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            await _publishChannel.ExchangeDeclareAsync(
                exchange: options.Exchange,
                type: options.ExchangeType,
                durable: options.Durable,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var body = serializer.Serialize(@event);

            var properties = new BasicProperties
            {
                MessageId = @event.Id.ToString(),
                Type = typeof(TEvent).FullName,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(new DateTimeOffset(@event.CreatedAtUtc).ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>()
            };

            await _publishChannel.BasicPublishAsync(
                exchange: options.Exchange,
                routingKey: options.RoutingKey,
                mandatory: options.Mandatory,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private async Task HandleMessageAsync(
        ISubscriptionDefinition subscription,
        IChannel consumerChannel,
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = ea.Body.ToArray();

            using var scope = scopeFactory.CreateScope();
            await subscription.HandleAsync(scope.ServiceProvider, body, cancellationToken);

            await consumerChannel.BasicAckAsync(
                ea.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while processing message. Queue={Queue}, Event={EventType}, Handler={HandlerType}",
                subscription.Queue,
                subscription.EventType.Name,
                subscription.HandlerType.Name);

            var retryCount = ReadRetryCount(ea.BasicProperties.Headers);

            if (subscription.EnableRetry && retryCount < subscription.MaxRetryCount)
            {
                await PublishToRetryQueueAsync(subscription, ea, retryCount + 1, cancellationToken);

                await consumerChannel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);

                return;
            }

            await consumerChannel.BasicNackAsync(
                ea.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);
        }
    }

    private async Task PublishToRetryQueueAsync(
        ISubscriptionDefinition subscription,
        BasicDeliverEventArgs ea,
        int retryCount,
        CancellationToken cancellationToken)
    {
        if (_publishChannel is null)
            throw new InvalidOperationException("Publish channel is not initialized.");

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            var headers = ea.BasicProperties.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(ea.BasicProperties.Headers);

            headers[RabbitMqHeaderNames.RetryCount] = retryCount;

            var properties = new BasicProperties
            {
                MessageId = ea.BasicProperties.MessageId,
                Type = ea.BasicProperties.Type,
                ContentType = ea.BasicProperties.ContentType,
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = ea.BasicProperties.Timestamp,
                Headers = headers
            };

            await _publishChannel.BasicPublishAsync(
                exchange: subscription.RetryExchange!,
                routingKey: subscription.RetryRoutingKey!,
                mandatory: false,
                basicProperties: properties,
                body: ea.Body,
                cancellationToken: cancellationToken);

            logger.LogWarning(
                "Message sent to retry queue. Queue={Queue}, RetryCount={RetryCount}, RetryQueue={RetryQueue}",
                subscription.Queue,
                retryCount,
                subscription.RetryQueue);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private async Task DeclareTopologyAsync(
        IChannel channel,
        ISubscriptionDefinition subscription,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: subscription.Exchange,
            type: subscription.ExchangeType,
            durable: subscription.Durable,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        Dictionary<string, object?>? mainQueueArguments = null;

        if (subscription.EnableDeadLetter)
        {
            mainQueueArguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = subscription.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = subscription.DeadLetterRoutingKey
            };
        }

        await channel.QueueDeclareAsync(
            queue: subscription.Queue,
            durable: subscription.Durable,
            exclusive: subscription.Exclusive,
            autoDelete: subscription.AutoDelete,
            arguments: mainQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: subscription.Queue,
            exchange: subscription.Exchange,
            routingKey: subscription.RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);

        if (subscription.EnableDeadLetter)
        {
            await channel.ExchangeDeclareAsync(
                exchange: subscription.DeadLetterExchange!,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: subscription.DeadLetterQueue!,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: subscription.DeadLetterQueue!,
                exchange: subscription.DeadLetterExchange!,
                routingKey: subscription.DeadLetterRoutingKey!,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        if (subscription.EnableRetry)
        {
            await channel.ExchangeDeclareAsync(
                exchange: subscription.RetryExchange!,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var retryQueueArguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = subscription.RetryTtlMilliseconds,
                ["x-dead-letter-exchange"] = subscription.Exchange,
                ["x-dead-letter-routing-key"] = subscription.RoutingKey
            };

            await channel.QueueDeclareAsync(
                queue: subscription.RetryQueue!,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryQueueArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: subscription.RetryQueue!,
                exchange: subscription.RetryExchange!,
                routingKey: subscription.RetryRoutingKey!,
                arguments: null,
                cancellationToken: cancellationToken);
        }
    }

    private static int ReadRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null)
            return 0;

        if (!headers.TryGetValue(RabbitMqHeaderNames.RetryCount, out var value) || value is null)
            return 0;

        return value switch
        {
            byte b => b,
            sbyte sb => sb,
            short s => s,
            ushort us => us,
            int i => i,
            long l => (int)l,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var result) => result,
            _ => 0
        };
    }

    private static void NormalizeAndValidate(RabbitMqSubscriptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Exchange))
            throw new ArgumentException("Exchange is required.");

        if (string.IsNullOrWhiteSpace(options.Queue))
            throw new ArgumentException("Queue is required.");

        if (string.IsNullOrWhiteSpace(options.RoutingKey))
            throw new ArgumentException("RoutingKey is required.");

        if (options.EnableDeadLetter)
        {
            options.DeadLetterExchange ??= $"{options.Queue}.dlx";
            options.DeadLetterQueue ??= $"{options.Queue}.dlq";
            options.DeadLetterRoutingKey ??= $"{options.Queue}.dlq";
        }

        if (options.EnableRetry)
        {
            options.RetryExchange ??= $"{options.Queue}.retry.exchange";
            options.RetryQueue ??= $"{options.Queue}.retry";
            options.RetryRoutingKey ??= $"{options.Queue}.retry";
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _publishLock.Dispose();
    }
}