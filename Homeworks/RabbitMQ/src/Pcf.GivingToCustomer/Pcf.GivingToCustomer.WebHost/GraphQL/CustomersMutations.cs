using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL;

[MutationType]
public class CustomersMutations
{
    public async Task<CustomerResponse> CreateCustomerAsync([Service] IRepository<Preference> preferenceRepository,
        [Service] IRepository<Customer> customerRepository, CreateOrEditCustomerRequest request)
    {
        var preferences = await preferenceRepository
            .GetRangeByIdsAsync(request.PreferenceIds);

        var customer = CustomerMapper.MapFromModel(request, preferences);

        await customerRepository.AddAsync(customer);

        return new CustomerResponse(customer);
    }


    public async Task<CustomerResponse> EditCustomerAsync([Service] IRepository<Customer> customerRepository,
        [Service] IRepository<Preference> preferenceRepository, Guid id, CreateOrEditCustomerRequest request)
    {
        var customer = await customerRepository.GetByIdAsync(id) ??
                       throw new GraphQLException(ErrorBuilder.New().SetMessage($"Customer {id} not found").Build());


        var preferences = await preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);

        CustomerMapper.MapFromModel(request, preferences, customer);

        await customerRepository.UpdateAsync(customer);

        return new CustomerResponse(customer);
    }


    public async Task<bool> DeleteCustomerAsync([Service] IRepository<Customer> customerRepository, Guid id)
    {
        var customer = await customerRepository.GetByIdAsync(id) ??
                       throw new GraphQLException(ErrorBuilder.New().SetMessage($"Customer {id} not found").Build());

        await customerRepository.DeleteAsync(customer);
        return true;
    }
}