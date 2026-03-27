using System;
using Microsoft.Extensions.Options;
using Pcf.GivingToCustomer.IntegrationTests.Data;

namespace Pcf.GivingToCustomer.IntegrationTests;

public class MongoDbFixture : IDisposable
{
    private readonly MongoTestDbInitializer _mongoDbInitializer;

    public MongoDbFixture()
    {
        var settings = TestMongoConfig.CreateSettings();
        DbContext = new TestDataContext(Options.Create(settings));
        _mongoDbInitializer = new MongoTestDbInitializer(DbContext);
        _mongoDbInitializer.InitializeDb();
    }

    public TestDataContext DbContext { get; }

    public void Dispose()
    {
        _mongoDbInitializer.CleanupDb();
    }
}