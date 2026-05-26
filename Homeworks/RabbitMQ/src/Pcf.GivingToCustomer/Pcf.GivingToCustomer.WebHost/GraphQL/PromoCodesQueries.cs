using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class PromoCodesQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<List<PromoCodeShortResponse>> GetPromocodesAsync(
        [Service] IRepository<PromoCode> promoCodeRepository)
    {
        var promocodes = await promoCodeRepository.GetAllAsync();

        return promocodes.Select(x => new PromoCodeShortResponse
        {
            Id = x.Id,
            Code = x.Code,
            BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
            EndDate = x.EndDate.ToString("yyyy-MM-dd"),
            PartnerId = x.PartnerId,
            ServiceInfo = x.ServiceInfo
        }).ToList();
    }
}