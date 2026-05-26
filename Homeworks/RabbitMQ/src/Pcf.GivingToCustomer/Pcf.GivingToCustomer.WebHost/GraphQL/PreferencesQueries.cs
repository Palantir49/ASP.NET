using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Data;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class PreferencesQueries
{
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 2)]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<PreferenceResponse>> GetPreferencesAsync(
        IRepository<Preference> preferencesRepository,
        CancellationToken cancellationToken)
    {
        var preferences = await preferencesRepository.GetAllAsync();

        return preferences.Select(element => new PreferenceResponse
        {
            Id = element.Id,
            Name = element.Name
        }).ToList();
    }
}