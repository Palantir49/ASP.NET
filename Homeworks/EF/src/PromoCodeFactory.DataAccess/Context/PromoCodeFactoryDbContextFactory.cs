using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PromoCodeFactory.DataAccess.Context;

public class PromoCodeFactoryDbContextFactory : IDesignTimeDbContextFactory<PromoCodeFactoryDbContext>
{
    public PromoCodeFactoryDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        // 1. Получаем конфигурацию из appsettings.json
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");
        builder.AddJsonFile($"appsettings.{env}.json");
        var configuration = builder.Build();

        // 2. Создаем DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<PromoCodeFactoryDbContext>();

        // 3. Получаем строку подключения
        var connectionString = configuration.GetConnectionString("Db");

        // 4. Настраиваем провайдер (SQLite)
        optionsBuilder.UseSqlite(connectionString);

        // 5. Возвращаем новый экземпляр контекста
        return new PromoCodeFactoryDbContext(optionsBuilder.Options);
    }
}