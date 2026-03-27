using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.DataAccess.Data;

public class MongoDbInitializer(MongoDbDataContext context) : IDbInitializer
{
    public void InitializeDb()
    {
        if (context.Customers.CountDocuments(Builders<Customer>.Filter.Empty) > 0)
            context.Customers.Database.DropCollection(context.Customers.CollectionNamespace.CollectionName);
        if (context.Preferences.CountDocuments(Builders<Preference>.Filter.Empty) > 0)
            context.Preferences.Database.DropCollection(context.Preferences.CollectionNamespace.CollectionName);
        if (context.PromoCodes.CountDocuments(Builders<PromoCode>.Filter.Empty) > 0)
            context.PromoCodes.Database.DropCollection(context.PromoCodes.CollectionNamespace.CollectionName);

        context.Customers.InsertMany(FakeDataFactory.Customers);
        context.Preferences.InsertMany(FakeDataFactory.Preferences);
    }
}