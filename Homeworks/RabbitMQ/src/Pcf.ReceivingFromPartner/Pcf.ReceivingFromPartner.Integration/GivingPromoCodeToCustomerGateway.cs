using System.Net.Http;
using System.Threading.Tasks;
using Pcf.MessageBus.Abstractions;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;
using Pcf.ReceivingFromPartner.Core.Domain;
using Pcf.ReceivingFromPartner.Integration.Dto;
using Pcf.ReceivingFromPartner.Integration.Mappers;

namespace Pcf.ReceivingFromPartner.Integration;

public class GivingPromoCodeToCustomerGateway(HttpClient httpClient, IEventBus eventBus)
    : IGivingPromoCodeToCustomerGateway
{
    public async Task GivePromoCodeToCustomer(PromoCode promoCode)
    {
        var dto = new GivePromoCodeToCustomerDto
        {
            PartnerId = promoCode.Partner.Id,
            BeginDate = promoCode.BeginDate.ToShortDateString(),
            EndDate = promoCode.EndDate.ToShortDateString(),
            PreferenceId = promoCode.PreferenceId,
            PromoCode = promoCode.Code,
            ServiceInfo = promoCode.ServiceInfo,
            PartnerManagerId = promoCode.PartnerManagerId
        };
        var @event = promoCode.ToPromoCodeReceivingIntegrationEvent();
        await eventBus.PublishAsync(@event);

        /*var response = await httpClient.PostAsJsonAsync("api/v1/promocodes", dto);

        response.EnsureSuccessStatusCode();*/
    }
}