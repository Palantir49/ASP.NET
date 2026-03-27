using Pcf.GivingToCustomer.DataAccess.Configirations;

namespace Pcf.GivingToCustomer.IntegrationTests;

public static class TestMongoConfig
{
    public static MongoDbSettings CreateSettings()
    {
        return new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "promocode_factory_givingToCustomer_db_test"
        };
    }
}