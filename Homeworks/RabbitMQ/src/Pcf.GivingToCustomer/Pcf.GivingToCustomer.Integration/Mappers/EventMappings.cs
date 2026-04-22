using Pcf.Events;
using Pcf.GivingToCustomer.Core.Dto;

namespace Pcf.GivingToCustomer.Integration.Mappers;

public static class EventMappings
{
    extension(PromoCodeReceivingIntegrationEvent promoCodeReceivingIntegrationEvent)
    {
        public PromoCodeDto ToDto()
        {
            return new PromoCodeDto
            {
                PartnerId = promoCodeReceivingIntegrationEvent.PartnerId,
                BeginDate = promoCodeReceivingIntegrationEvent.BeginDate,
                EndDate = promoCodeReceivingIntegrationEvent.EndDate,
                PreferenceId = promoCodeReceivingIntegrationEvent.PreferenceId,
                PromoCode = promoCodeReceivingIntegrationEvent.PromoCode,
                ServiceInfo = promoCodeReceivingIntegrationEvent.ServiceInfo,
                PartnerManagerId = promoCodeReceivingIntegrationEvent.PartnerManagerId
            };
        }
    }
}