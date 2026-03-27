using Microsoft.Extensions.Options;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Configirations;

namespace Pcf.GivingToCustomer.IntegrationTests;

public class TestDataContext(IOptions<MongoDbSettings> mongoDbSettings) : MongoDbDataContext(mongoDbSettings);