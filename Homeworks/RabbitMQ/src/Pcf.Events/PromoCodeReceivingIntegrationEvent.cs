using Pcf.MessageBus.Abstractions;

namespace Pcf.Events;

public sealed record PromoCodeReceivingIntegrationEvent : IntegrationEvent
{
    public string? ServiceInfo { get; set; }

    public Guid PartnerId { get; set; }

    public Guid PromoCodeId { get; set; }

    public string? PromoCode { get; set; }

    public Guid PreferenceId { get; set; }

    public string? BeginDate { get; set; }

    public string? EndDate { get; set; }
}