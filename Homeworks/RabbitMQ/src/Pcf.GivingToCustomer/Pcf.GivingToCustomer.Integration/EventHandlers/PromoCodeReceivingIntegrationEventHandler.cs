using System.Threading;
using System.Threading.Tasks;
using Pcf.Events;
using Pcf.GivingToCustomer.Core.Abstractions.Services;
using Pcf.GivingToCustomer.Integration.Mappers;
using Pcf.MessageBus.Abstractions;

namespace Pcf.GivingToCustomer.Integration.EventHandlers;

public sealed class PromoCodeReceivingIntegrationEventHandler(IPromoCodeService promoCodeService)
    : IEventHandler<PromoCodeReceivingIntegrationEvent>
{
    public async Task Handle(
        PromoCodeReceivingIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var promoCodeDto = @event.ToDto();
        await promoCodeService.GivePromoCodesToCustomersWithPreferenceAsync(promoCodeDto);
    }
}