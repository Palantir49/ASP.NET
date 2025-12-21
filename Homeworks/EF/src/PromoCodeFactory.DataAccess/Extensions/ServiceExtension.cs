using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromoCodeFactory.DataAccess.Context;
using PromoCodeFactory.DataAccess.Repositories.Implements;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;

namespace PromoCodeFactory.DataAccess.Extensions;

public static class ServiceExtension
{
    public static void ConfigureDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Db");
        services.AddDbContext<PromoCodeFactoryDbContext>(options =>
            {
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
            }
        );
        services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
    }
}