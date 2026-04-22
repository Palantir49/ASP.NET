using System.Threading.Tasks;
using Pcf.GivingToCustomer.Core.Dto;

namespace Pcf.GivingToCustomer.Core.Abstractions.Services;

public interface IPromoCodeService
{
    Task GivePromoCodesToCustomersWithPreferenceAsync(PromoCodeDto promoCodeDto);
}