using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.WebHost.Models;

public class GivePromoCodeRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "ServiceInfo не может быть пустым")]
    public string ServiceInfo { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "PartnerName не может быть пустым")]
    public string PartnerName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Промокод не может быть пустым")]
    public string PromoCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Preference не может быть пустым")]
    public string Preference { get; set; }
}