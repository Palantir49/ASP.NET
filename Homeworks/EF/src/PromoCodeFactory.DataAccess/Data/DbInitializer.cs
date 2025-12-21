using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PromoCodeFactory.DataAccess.Context;

namespace PromoCodeFactory.DataAccess.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PromoCodeFactoryDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.MigrateAsync();
        Seed(dbContext);
    }

    private static void Seed(PromoCodeFactoryDbContext dbContext)
    {
        if (!dbContext.Employees.Any()) dbContext.Employees.AddRange(FakeDataFactory.Employees);
        if (!dbContext.PromoCodes.Any()) dbContext.Customers.AddRange(FakeDataFactory.Customers);
        dbContext.SaveChanges();
    }
}