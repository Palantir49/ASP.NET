using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;

namespace Pcf.GivingToCustomer.IntegrationTests.Data;

public class MongoTestDbInitializer(MongoDbDataContext context) : IDbInitializer
{
    public void InitializeDb()
    {
        CleanupDb();

        context.Customers.InsertMany(TestDataFactory.Customers);
        context.Preferences.InsertMany(TestDataFactory.Preferences);
    }

    public void CleanupDb()
    {
        if (context.Customers.CountDocuments(Builders<Customer>.Filter.Empty) > 0)
            context.Customers.Database.DropCollection(context.Customers.CollectionNamespace.CollectionName);
        if (context.Preferences.CountDocuments(Builders<Preference>.Filter.Empty) > 0)
            context.Preferences.Database.DropCollection(context.Preferences.CollectionNamespace.CollectionName);
        if (context.PromoCodes.CountDocuments(Builders<PromoCode>.Filter.Empty) > 0)
            context.PromoCodes.Database.DropCollection(context.PromoCodes.CollectionNamespace.CollectionName);
    }
}