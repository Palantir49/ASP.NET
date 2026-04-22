using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Abstractions.Services;
using Pcf.Administration.Core.Realizations.Services;
using Pcf.Administration.DataAccess;
using Pcf.Administration.DataAccess.Data;
using Pcf.Administration.DataAccess.Repositories;
using Pcf.Administration.Integration.EventHandlers;
using Pcf.Events;
using Pcf.MessageBus.RabbitMq;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Pcf.Administration.WebHost;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        //add rabbitmq consumer
        services.AddRabbitMqConsumer(Configuration);
        services.AddRabbitMqSubscription<
            PartnerManagerReceivingIntegrationEvent,
            PartnerManagerReceivingIntegrationEventHandler>(
            Configuration,
            "partner-manager-received");

        services.AddControllers().AddMvcOptions(x =>
            x.SuppressAsyncSuffixInActionNames = false);
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IDbInitializer, EfDbInitializer>();
        services.AddDbContext<DataContext>(x =>
        {
            //x.UseSqlite("Filename=PromocodeFactoryAdministrationDb.sqlite");
            x.UseNpgsql(Configuration.GetConnectionString("PromocodeFactoryAdministrationDb"));
            x.UseSnakeCaseNamingConvention();
            x.UseLazyLoadingProxies();
        });

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddOpenApiDocument(options =>
        {
            options.Title = "PromoCode Factory Administration API Doc";
            options.Version = "1.0";
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseHsts();

        app.UseOpenApi();
        app.UseSwaggerUi(x => { x.DocExpansion = "list"; });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        dbInitializer.InitializeDb();
    }
}