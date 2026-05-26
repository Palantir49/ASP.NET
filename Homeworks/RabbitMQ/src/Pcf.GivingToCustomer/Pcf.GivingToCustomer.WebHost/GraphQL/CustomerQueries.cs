using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class CustomerQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public async Task<List<CustomerShortResponse>> GetCustomersAsync([Service] IRepository<Customer> customerRepository,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.GetAllAsync();

        return customers.Select(x => new CustomerShortResponse
        {
            Id = x.Id,
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName
        }).ToList();
    }


    public async Task<CustomerResponse> GetCustomerAsync([Service] IRepository<Customer> customerRepository, Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(id);

        return new CustomerResponse(customer);
    }
}