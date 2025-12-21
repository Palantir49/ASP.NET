using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement;

public class Customer
    : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; }

    // one client can have many promocodes
    public ICollection<PromoCode> PromoCodes { get; set; }

    // one client can have many preferences
    public ICollection<Preference> Preferences { get; set; }
}