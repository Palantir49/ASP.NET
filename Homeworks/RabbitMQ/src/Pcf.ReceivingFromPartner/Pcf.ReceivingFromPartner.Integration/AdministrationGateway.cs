using System;
using System.Net.Http;
using System.Threading.Tasks;
using Pcf.Events;
using Pcf.MessageBus.Abstractions;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;

namespace Pcf.ReceivingFromPartner.Integration;

public class AdministrationGateway(HttpClient httpClient, IEventBus eventBus) : IAdministrationGateway
{
    public async Task NotifyAdminAboutPartnerManagerPromoCode(Guid partnerManagerId)
    {
        var @event = new PartnerManagerReceivingIntegrationEvent
        {
            PartnerManagerId = partnerManagerId
        };

        await eventBus.PublishAsync(@event);
        /* var response = await httpClient.PostAsync($"api/v1/employees/{partnerManagerId}/appliedPromocodes",
             new StringContent(string.Empty));

         response.EnsureSuccessStatusCode();*/
    }
}