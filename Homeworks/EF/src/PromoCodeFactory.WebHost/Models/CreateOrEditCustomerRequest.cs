using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.WebHost.Models;

public class CreateOrEditCustomerRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Имя не может быть пустым")]
    public string FirstName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Фамилия не может быть пустой")]
    public string LastName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат Email")]
    public string Email { get; set; }

    public List<Guid> PreferenceIds { get; set; }
}