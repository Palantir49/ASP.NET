using System;

namespace PromoCodeFactory.Core.Domain.Administration;

public class Employee
    : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; }

    // one to many (one role to many employees)
    public Role Role { get; set; }

    // ВНЕШНИЙ КЛЮЧ - указывает на роль таблице Roles
    public Guid? RoleId { get; set; }

    public int AppliedPromocodesCount { get; set; }
}