using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.Integration;
using Pcf.GivingToCustomer.IntegrationTests.Data;

namespace Pcf.GivingToCustomer.IntegrationTests;

public class TestWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    public IConfiguration Configuration { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<INotificationGateway, NotificationGateway>();

            services.AddSingleton(sp =>
            {
                var settings = TestMongoConfig.CreateSettings();
                return new MongoDbDataContext(Options.Create(settings));
            });
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<MongoDbDataContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

            try
            {
                new MongoTestDbInitializer(dbContext).InitializeDb();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Проблема во время заполнения тестовой базы. " +
                                    "Ошибка: {Message}", ex.Message);
            }
        });
    }
}