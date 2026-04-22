using Pcf.Administration.Core.Abstractions.Services;
using Pcf.Events;
using Pcf.MessageBus.Abstractions;

namespace Pcf.Administration.Integration.EventHandlers;

public class PartnerManagerReceivingIntegrationEventHandler(IEmployeeService employeeService)
    : IEventHandler<PartnerManagerReceivingIntegrationEvent>
{
    public async Task Handle(PartnerManagerReceivingIntegrationEvent @event, CancellationToken cancellationToken)
    {
        await employeeService.UpdateAppliedPromoCodesAsync(@event.PartnerManagerId);
    }
}