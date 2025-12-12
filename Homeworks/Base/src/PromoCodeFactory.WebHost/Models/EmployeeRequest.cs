using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.WebHost.Models;

public record EmployeeRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Имя не может быть пустым")]
    string FirstName,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Фамилия не может быть пустой")]
    string LastName,
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат Email")]
    string Email,
    [Required(ErrorMessage = "Роль обязательна")]
    List<string> Roles,
    [Range(0, int.MaxValue, ErrorMessage = "Количество промокодов должно быть неотрицательным")]
    int AppliedPromocodesCount);