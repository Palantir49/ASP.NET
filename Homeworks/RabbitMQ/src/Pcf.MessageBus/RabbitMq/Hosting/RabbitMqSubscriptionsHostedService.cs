using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pcf.MessageBus.Abstractions;

namespace Pcf.MessageBus.RabbitMq.Hosting;

public sealed class RabbitMqSubscriptionsHostedService(
    IEventBus eventBus,
    IEnumerable<IEventBusSubscriptionConfigurator> configurators,
    ILogger<RabbitMqSubscriptionsHostedService> logger)
    : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var configurator in configurators)
        {
            configurator.Configure(eventBus);
        }

        await eventBus.StartAsync(cancellationToken);
        logger.LogInformation("RabbitMQ event bus hosted service started");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await eventBus.StopAsync(cancellationToken);
        logger.LogInformation("RabbitMQ event bus hosted service stopped");

        await base.StopAsync(cancellationToken);
    }
}