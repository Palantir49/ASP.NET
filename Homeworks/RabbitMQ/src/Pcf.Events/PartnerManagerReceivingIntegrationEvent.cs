using Pcf.MessageBus.Abstractions;

namespace Pcf.Events;

public sealed record PartnerManagerReceivingIntegrationEvent : IntegrationEvent
{
    public Guid PartnerManagerId { get; set; }
}