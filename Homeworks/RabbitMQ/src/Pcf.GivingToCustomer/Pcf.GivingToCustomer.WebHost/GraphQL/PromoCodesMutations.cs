using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL;

[MutationType]
public class PromoCodesMutations
{
    public async Task<bool> GivePromoCodesToCustomersWithPreferenceAsync(
        [Service] IRepository<Preference> preferencesRepository, [Service] IRepository<Customer> customersRepository,
        [Service] IRepository<PromoCode> promoCodesRepository,
        GivePromoCodeRequest request)
    {
        //Получаем предпочтение по имени
        var preference = await preferencesRepository.GetByIdAsync(request.PreferenceId) ??
                         throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage($"Preference {request.PreferenceId} not found").Build());


        //  Получаем клиентов с этим предпочтением:
        var customers = await customersRepository
            .GetWhere(d => d.Preferences.Any(x =>
                x.Preference.Id == preference.Id));

        var promoCode = PromoCodeMapper.MapFromModel(request, preference, customers);

        await promoCodesRepository.AddAsync(promoCode);

        return true;
    }
}