using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Configirations;
using Pcf.GivingToCustomer.DataAccess.Data;
using Pcf.GivingToCustomer.DataAccess.Repositories;
using Pcf.GivingToCustomer.Integration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Pcf.GivingToCustomer.WebHost;

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
        services.AddControllers().AddMvcOptions(x =>
            x.SuppressAsyncSuffixInActionNames = false);
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        // services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(MongoDbRepository<>));
        services.AddScoped<INotificationGateway, NotificationGateway>();
        services.AddScoped<IDbInitializer, MongoDbInitializer>();
        services.Configure<MongoDbSettings>(Configuration.GetSection("PromocodeFactoryGivingToCustomerMongoDb"));
        services.AddSingleton<MongoDbDataContext>();

        services.AddOpenApiDocument(options =>
        {
            options.Title = "PromoCode Factory Giving To Customer API Doc";
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