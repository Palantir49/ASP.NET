using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using PromoCodes;

namespace Pcf.GivingToCustomer.WebHost.Grpc.Services;

public class PromoCodesService(
    IRepository<PromoCode> promoCodesRepository,
    IRepository<Preference> preferencesRepository,
    IRepository<Customer> customersRepository) : PromoCodes.PromoCodesService.PromoCodesServiceBase
{
    public override async Task<PromoCodeShortResponse> GetPromoCodesAsync(Empty request, ServerCallContext context)
    {
        var promoCodes = await promoCodesRepository.GetAllAsync();

        return new PromoCodeShortResponse
        {
            PromoCode =
            {
                promoCodes.Select(element => new PromoCodeShort
                {
                    BeginDate = element.BeginDate.ToString("yyyy-MM-dd"),
                    EndDate = element.EndDate.ToString("yyyy-MM-dd"),
                    Code = element.Code,
                    ServiceInfo = element.ServiceInfo,
                    Id = element.Id.ToString(),
                    PartnerId = element.PartnerId.ToString()
                }).ToList()
            }
        };
    }

    public override async Task<Empty> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request,
        ServerCallContext context)
    {
        var id = Guid.TryParse(request.PreferenceId, out var guid)
            ? guid
            : throw new ArgumentException("Invalid PreferenceId");
        //Получаем предпочтение по имени
        var preference = await preferencesRepository.GetByIdAsync(id);

        if (preference == null) throw new KeyNotFoundException($"Preference {id} not found");

        //  Получаем клиентов с этим предпочтением:
        var customers = await customersRepository
            .GetWhere(d => d.Preferences.Any(x =>
                x.Preference.Id == preference.Id));
        var givePromoCodeRequest = new Models.GivePromoCodeRequest
        {
            PromoCode = request.PromoCode,
            ServiceInfo = request.ServiceInfo,
            PartnerId = Guid.Parse(request.PartnerId),
            PromoCodeId = Guid.Parse(request.PromoCodeId),
            BeginDate = request.BeginDate,
            EndDate = request.EndDate,
            PreferenceId = preference.Id
        };
        var promoCode = PromoCodeMapper.MapFromModel(givePromoCodeRequest, preference, customers);

        await promoCodesRepository.AddAsync(promoCode);
        return new Empty();
    }
}