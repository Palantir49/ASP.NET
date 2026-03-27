using System;
using System.Collections.Generic;

namespace Pcf.GivingToCustomer.Core.Domain
{
    public class Customer
        :BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        // вместо CustomerPreference: просто список preferenceId
        public List<Guid> PreferenceIds { get; set; } = [];
    
        // если нужны промокоды у клиента – тоже ids
        public List<Guid> PromoCodeIds { get; set; } = [];
    }
}