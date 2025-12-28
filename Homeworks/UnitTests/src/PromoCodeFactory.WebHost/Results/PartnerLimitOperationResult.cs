using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.WebHost.Results;

public class PartnerLimitOperationResult
{
    private PartnerLimitOperationResult(bool isSuccess, string errorMessage = null,
        PartnerPromoCodeLimit partnerPromoCodeLimit = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        NewLimitId = partnerPromoCodeLimit;
    }

    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    public PartnerPromoCodeLimit NewLimitId { get; }

    public static PartnerLimitOperationResult Success(PartnerPromoCodeLimit partnerPromoCodeLimit)
    {
        return new PartnerLimitOperationResult(true, partnerPromoCodeLimit: partnerPromoCodeLimit);
    }

    public static PartnerLimitOperationResult Failure(string errorMessage)
    {
        return new PartnerLimitOperationResult(false, errorMessage);
    }
}