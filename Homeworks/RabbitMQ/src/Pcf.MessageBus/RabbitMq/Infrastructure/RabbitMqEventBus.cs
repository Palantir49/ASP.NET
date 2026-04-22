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

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private readonly List<(IChannel Channel, string ConsumerTag)> _consumerChannels = [];
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private readonly IEventSerializer _serializer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IReadOnlyCollection<ISubscriptionDefinition> _subscriptions;
    private readonly IEventTopologyRegistry _topologyRegistry;

    private IConnection? _connection;
    private bool _consumersStarted;
    private IChannel? _publishChannel;

    public RabbitMqEventBus(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<RabbitMqOptions> options,
        IEventTopologyRegistry topologyRegistry,
        IEnumerable<IRabbitMqSubscriptionDescriptor> subscriptionDescriptors,
        IEventSerializer serializer,
        ILogger<RabbitMqEventBus> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _topologyRegistry = topologyRegistry;
        _serializer = serializer;
        _logger = logger;
        _options = options.Value;
        _subscriptions = subscriptionDescriptors.Select(x => x.Create()).ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _publishLock.Dispose();
        _connectionLock.Dispose();
    }

    public void Subscribe<TEvent, THandler>(Action<SubscriptionBuilder> configure) where TEvent : IntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        throw new NotSupportedException(
            "Use IServiceCollection.AddRabbitMqSubscription<TEvent, THandler>(configuration, subscriptionName) during registration.");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_consumersStarted)
            return;

        await EnsureConnectionAsync(cancellationToken);

        foreach (var subscription in _subscriptions)
        {
            var channel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);

            await DeclareTopologyAsync(channel, subscription, cancellationToken);
            await channel.BasicQosAsync(0, subscription.PrefetchCount, false, cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                await HandleMessageAsync(subscription, channel, ea, cancellationToken);
            };

            var consumerTag = await channel.BasicConsumeAsync(
                subscription.Queue,
                false,
                consumer,
                cancellationToken);

            _consumerChannels.Add((channel, consumerTag));

            _logger.LogInformation(
                "Consumer started. Event={EventType}, Handler={HandlerType}, Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                subscription.EventType.Name,
                subscription.HandlerType.Name,
                subscription.Queue,
                subscription.Exchange,
                subscription.RoutingKey);
        }

        _consumersStarted = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (channel, consumerTag) in _consumerChannels)
            try
            {
                await channel.BasicCancelAsync(consumerTag, cancellationToken: cancellationToken);
                await channel.CloseAsync(cancellationToken);
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping consumer channel.");
            }

        _consumerChannels.Clear();
        _consumersStarted = false;

        if (_publishChannel is not null)
        {
            try
            {
                await _publishChannel.CloseAsync(cancellationToken);
                await _publishChannel.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping publish channel.");
            }

            _publishChannel = null;
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while closing RabbitMQ connection.");
            }

            _connection = null;
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        await EnsurePublishInfrastructureAsync(cancellationToken);

        var options = _topologyRegistry.GetPublishOptions<TEvent>();

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            await _publishChannel!.ExchangeDeclareAsync(
                options.Exchange,
                options.ExchangeType,
                options.Durable,
                false,
                cancellationToken: cancellationToken);

            var body = _serializer.Serialize(@event);

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
                options.Exchange,
                options.RoutingKey,
                options.Mandatory,
                properties,
                body,
                cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        throw new NotSupportedException(
            "Use IServiceCollection.AddRabbitMqSubscription<TEvent, THandler>(configuration, subscriptionName) during registration.");
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
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
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsOpen)
            return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null && _connection.IsOpen)
                return;

            var factory = CreateConnectionFactory();
            _connection = await factory.CreateConnectionAsync(cancellationToken);

            _logger.LogInformation(
                "RabbitMQ connection established. ClientName={ClientProvidedName}",
                _options.Connection.ClientProvidedName);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task EnsurePublishChannelAsync(CancellationToken cancellationToken)
    {
        if (_publishChannel is not null && _publishChannel.IsOpen)
            return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_publishChannel is not null && _publishChannel.IsOpen)
                return;

            if (_connection is null || !_connection.IsOpen)
            {
                var factory = CreateConnectionFactory();
                _connection = await factory.CreateConnectionAsync(cancellationToken);
            }

            _publishChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            _publishChannel.BasicReturnAsync += async (_, args) =>
            {
                _logger.LogWarning(
                    "Message returned by broker. Exchange={Exchange}, RoutingKey={RoutingKey}, ReplyCode={ReplyCode}, ReplyText={ReplyText}",
                    args.Exchange,
                    args.RoutingKey,
                    args.ReplyCode,
                    args.ReplyText);

                await Task.CompletedTask;
            };
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task EnsurePublishInfrastructureAsync(CancellationToken cancellationToken)
    {
        await EnsureConnectionAsync(cancellationToken);
        await EnsurePublishChannelAsync(cancellationToken);
    }

    private async Task DeclareTopologyAsync(
        IChannel channel,
        ISubscriptionDefinition subscription,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            subscription.Exchange,
            subscription.ExchangeType,
            subscription.Durable,
            false,
            cancellationToken: cancellationToken);

        IDictionary<string, object?>? queueArguments = null;

        if (subscription.EnableDeadLetter)
            queueArguments = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = subscription.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = subscription.DeadLetterRoutingKey
            };

        await channel.QueueDeclareAsync(
            subscription.Queue,
            subscription.Durable,
            subscription.Exclusive,
            subscription.AutoDelete,
            queueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            subscription.Queue,
            subscription.Exchange,
            subscription.RoutingKey,
            cancellationToken: cancellationToken);

        if (subscription.EnableRetry)
        {
            var retryQueueArguments = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = subscription.RetryTtlMilliseconds,
                ["x-dead-letter-exchange"] = subscription.Exchange,
                ["x-dead-letter-routing-key"] = subscription.RoutingKey
            };

            await channel.ExchangeDeclareAsync(
                subscription.RetryExchange!,
                "direct",
                true,
                false,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                subscription.RetryQueue!,
                true,
                false,
                false,
                retryQueueArguments,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                subscription.RetryQueue!,
                subscription.RetryExchange!,
                subscription.RetryRoutingKey!,
                cancellationToken: cancellationToken);
        }

        if (subscription.EnableDeadLetter)
        {
            await channel.ExchangeDeclareAsync(
                subscription.DeadLetterExchange!,
                "direct",
                true,
                false,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                subscription.DeadLetterQueue!,
                true,
                false,
                false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                subscription.DeadLetterQueue!,
                subscription.DeadLetterExchange!,
                subscription.DeadLetterRoutingKey!,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleMessageAsync(
        ISubscriptionDefinition subscription,
        IChannel channel,
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = ea.Body.ToArray();

            using var scope = _serviceScopeFactory.CreateScope();
            await subscription.HandleAsync(scope.ServiceProvider, body, cancellationToken);

            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Message processing failed. Queue={Queue}, EventType={EventType}",
                subscription.Queue,
                subscription.EventType.Name);

            await channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
        }
    }
}