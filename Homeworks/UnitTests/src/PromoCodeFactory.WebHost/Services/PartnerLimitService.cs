using System;
using System.Linq;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Results;

namespace PromoCodeFactory.WebHost.Services;

public class PartnerLimitService
{
    public PartnerLimitOperationResult ProcessLimitAsync(
        Partner partner,
        SetPartnerPromoCodeLimitRequest request,
        DateTime currentTime)
    {
        // Валидация лимита
        if (request.Limit <= 0)
            return PartnerLimitOperationResult.Failure("Лимит должен быть больше 0");

        var activeLimit = partner.PartnerLimits.FirstOrDefault(x => !x.CancelDate.HasValue);

        if (activeLimit != null)
        {
            partner.NumberIssuedPromoCodes = 0;
            activeLimit.CancelDate = currentTime;
        }

        var newLimit = new PartnerPromoCodeLimit
        {
            Limit = request.Limit,
            Partner = partner,
            PartnerId = partner.Id,
            CreateDate = currentTime,
            EndDate = request.EndDate
        };

        partner.PartnerLimits.Add(newLimit);

        return PartnerLimitOperationResult.Success(newLimit);
    }
}