using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Abstractions.Services;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Core.Dto;
using Pcf.GivingToCustomer.Core.Mappers;

namespace Pcf.GivingToCustomer.Core.Realizations;

public sealed class PromoCodeService(
    IRepository<Preference> preferencesRepository,
    IRepository<PromoCode> promoCodesRepository,
    IRepository<Customer> customersRepository) : IPromoCodeService
{
    public async Task GivePromoCodesToCustomersWithPreferenceAsync(PromoCodeDto promoCodeDto)
    {
        //Получаем предпочтение по имени
        var preference = await preferencesRepository.GetByIdAsync(promoCodeDto.PreferenceId);

        if (preference == null)
            throw new KeyNotFoundException($"Предпочтение с данным id {promoCodeDto.PreferenceId} не найдено");

        //  Получаем клиентов с этим предпочтением:
        var customers = await customersRepository
            .GetWhere(d => d.Preferences.Any(x =>
                x.Preference.Id == preference.Id));

        var promoCode = promoCodeDto.ToModel(preference, customers);

        await promoCodesRepository.AddAsync(promoCode);
    }
}