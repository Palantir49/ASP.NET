using System.ComponentModel.DataAnnotations;

namespace Pcf.GivingToCustomer.DataAccess.Configirations;

public record MongoDbSettings
{   
     public required string  ConnectionString { get; init; }
     public required string DatabaseName {get; init;}
    
}
