using System.Collections.Generic;

namespace PromoCodeFactory.Core.Domain.PromoCodeManagement;

public class Preference
    : BaseEntity
{
    public string Name { get; set; }

    // Обратная связь Many-to-Many
    public ICollection<Customer> Customers { get; set; }
}