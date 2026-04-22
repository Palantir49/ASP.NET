using Pcf.Events;
using Pcf.ReceivingFromPartner.Core.Domain;

namespace Pcf.ReceivingFromPartner.Integration.Mappers;

public static class EventMappings
{
    extension(PromoCode promoCode)
    {
        public PromoCodeReceivingIntegrationEvent ToPromoCodeReceivingIntegrationEvent()
        {
            return new PromoCodeReceivingIntegrationEvent
            {
                PartnerId = promoCode.Partner.Id,
                BeginDate = promoCode.BeginDate.ToShortDateString(),
                EndDate = promoCode.EndDate.ToShortDateString(),
                PreferenceId = promoCode.PreferenceId,
                PromoCode = promoCode.Code,
                ServiceInfo = promoCode.ServiceInfo,
                PartnerManagerId = promoCode.PartnerManagerId
            };
        }
    }
}