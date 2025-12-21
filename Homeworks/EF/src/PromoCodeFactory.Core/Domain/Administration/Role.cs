using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.Administration;

public class Role
    : BaseEntity
{
    public string Name { get; set; }

    public string Description { get; set; }

    // one to many (one role to many employees)
    public ICollection<Employee> Employees { get; set; }
}