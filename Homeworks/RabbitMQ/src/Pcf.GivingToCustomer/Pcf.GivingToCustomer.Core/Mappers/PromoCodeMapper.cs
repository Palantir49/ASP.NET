using System;
using System.Collections.Generic;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Core.Dto;

namespace Pcf.GivingToCustomer.Core.Mappers;

public static class PromoCodeMapper
{
    extension(PromoCodeDto promoCodeDto)
    {
        public PromoCode ToModel(Preference preference, IEnumerable<Customer> customers)
        {
            var promoCode = new PromoCode
            {
                Id = promoCodeDto.PromoCodeId,
                PartnerId = promoCodeDto.PartnerId,
                Code = promoCodeDto.PromoCode,
                ServiceInfo = promoCodeDto.ServiceInfo,
                BeginDate = DateTime.Parse(promoCodeDto.BeginDate),
                EndDate = DateTime.Parse(promoCodeDto.EndDate),
                Preference = preference,
                PreferenceId = preference.Id,
                Customers = new List<PromoCodeCustomer>()
            };

            foreach (var item in customers)
                promoCode.Customers.Add(new PromoCodeCustomer
                {
                    CustomerId = item.Id,
                    Customer = item,
                    PromoCodeId = promoCode.Id,
                    PromoCode = promoCode
                });

            return promoCode;
        }
    }
}