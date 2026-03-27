using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess.Configirations;

namespace Pcf.GivingToCustomer.DataAccess;

public class MongoDbDataContext
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoDbDataContext(IOptions<MongoDbSettings> mongoDbSettings)
    {
        _client = new MongoClient(mongoDbSettings.Value.ConnectionString);
        _database = _client.GetDatabase(mongoDbSettings.Value.DatabaseName);
    }


    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
    public IMongoCollection<Preference> Preferences => _database.GetCollection<Preference>("Preferences");
    public IMongoCollection<PromoCode> PromoCodes => _database.GetCollection<PromoCode>("PromoCodes");
}