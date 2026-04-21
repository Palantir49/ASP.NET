using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pcf.MessageBus.Abstractions;
using Pcf.MessageBus.RabbitMq.Hosting;
using Pcf.MessageBus.RabbitMq.Infrastructure;
using Pcf.MessageBus.RabbitMq.Options;
using Pcf.MessageBus.RabbitMq.Serialization;
using Pcf.MessageBus.RabbitMq.Topology;

namespace Pcf.MessageBus.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqEventBusCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IEventTopologyRegistry, RabbitMqTopologyRegistry>();
        services.TryAddSingleton<IRabbitMqSubscriptionOptionsProvider, RabbitMqSubscriptionOptionsProvider>();
        services.TryAddSingleton<IEventSerializer, SystemTextJsonEventSerializer>();
        services.TryAddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }

    public static IServiceCollection AddRabbitMqEventPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRabbitMqEventBusCore(configuration);
        return services;
    }

    public static IServiceCollection AddRabbitMqEventConsumers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRabbitMqEventBusCore(configuration);
        services.AddHostedService<RabbitMqSubscriptionsHostedService>();
        return services;
    }

    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRabbitMqEventPublisher(configuration);
        services.AddRabbitMqEventConsumers(configuration);
        return services;
    }
}