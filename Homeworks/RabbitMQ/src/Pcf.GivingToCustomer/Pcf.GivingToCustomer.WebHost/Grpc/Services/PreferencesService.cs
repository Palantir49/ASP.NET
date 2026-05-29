using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Preferences;
using Preference = Pcf.GivingToCustomer.Core.Domain.Preference;

namespace Pcf.GivingToCustomer.WebHost.Grpc.Services;

public class PreferencesService(IRepository<Preference> preferencesRepository)
    : Preferences.PreferencesService.PreferencesServiceBase
{
    public override async Task<PreferenceResponse> GetPreferencesAsync(Empty request, ServerCallContext context)
    {
        var preferences = await preferencesRepository.GetAllAsync();

        return new PreferenceResponse
        {
            Preferences =
            {
                preferences.Select(element => new PreferenceShort
                {
                    Id = element.Id.ToString(),
                    Name = element.Name
                }).ToList()
            }
        };
    }
}