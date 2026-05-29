using System;
using System.Linq;
using System.Threading.Tasks;
using Customers;
using Google.Protobuf.WellKnownTypes;
using GreenDonut;
using Grpc.Core;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using CreateOrEditCustomerRequest = Pcf.GivingToCustomer.WebHost.Models.CreateOrEditCustomerRequest;

namespace Pcf.GivingToCustomer.WebHost.Grpc.Services;

public class CustomersService(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository)
    : Customers.CustomersService.CustomersServiceBase
{
    public override async Task<CustomerShortResponse> GetCustomersAsync(Empty request, ServerCallContext context)
    {
        var customers = await customerRepository.GetAllAsync();
        return new CustomerShortResponse
        {
            CustomerShort =
            {
                customers.Select(x => new CustomerShort
                {
                    Id = x.Id.ToString(),
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName
                }).ToList()
            }
        };
    }

    public override async Task<CustomerResponse> GetCustomerAsync(CustomerRequest request, ServerCallContext context)
    {
        var id = Guid.TryParse(request.Id, out var guid)
            ? guid
            : throw new ArgumentException("Invalid CustomerId");
        var customer = await customerRepository.GetByIdAsync(id);

        return new CustomerResponse
        {
            Id = customer.Id.ToString(),
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Preferences =
            {
                customer.Preferences.Select(element => new PreferenceResponse
                {
                    Id = element.PreferenceId.ToString(),
                    Name = element.Preference.Name
                })
            },
            PromoCodes =
            {
                customer.PromoCodes.Select(element => new PromoCodeShortResponse
                {
                    Id = element.PromoCodeId.ToString(),
                    BeginDate = element.PromoCode.BeginDate.ToString("yyyy-MM-dd"),
                    Code = element.PromoCode.Code,
                    EndDate = element.PromoCode.EndDate.ToString("yyyy-MM-dd"),
                    PartnerId = element.PromoCode.PartnerId.ToString(),
                    ServiceInfo = element.PromoCode.ServiceInfo
                })
            }
        };
    }

    public override async Task<CustomerResponse> CreateCustomerAsync(Customers.CreateOrEditCustomerRequest request,
        ServerCallContext context)
    {
        var preferencesIds = request.PreferenceIds.Select(Guid.Parse).ToList();
        var preferences = await preferenceRepository
            .GetRangeByIdsAsync(preferencesIds);

        var createOrEditCustomerRequest = new CreateOrEditCustomerRequest
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PreferenceIds = preferencesIds
        };

        var customer = CustomerMapper.MapFromModel(createOrEditCustomerRequest, preferences);

        await customerRepository.AddAsync(customer);

        return new CustomerResponse
        {
            Id = customer.Id.ToString(),
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Preferences =
            {
                customer.Preferences.Select(element => new PreferenceResponse
                {
                    Id = element.PreferenceId.ToString(),
                    Name = element.Preference.Name
                })
            }
        };
    }


    public override async Task<Empty> EditCustomersAsync(EditCustomerRequest request, ServerCallContext context)
    {
        var id = Guid.TryParse(request.Id, out var guid)
            ? guid
            : throw new ArgumentException("Invalid CustomerId");
        var customer = await customerRepository.GetByIdAsync(id);

        if (customer == null)
            throw new KeyNotFoundException($"Customer {request.Id} not found");

        var preferencesIds = request.Request.PreferenceIds.Select(Guid.Parse).ToList();

        var preferences = await preferenceRepository.GetRangeByIdsAsync(preferencesIds);
        var createOrEditCustomerRequest = new CreateOrEditCustomerRequest
        {
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PreferenceIds = preferencesIds
        };
        CustomerMapper.MapFromModel(createOrEditCustomerRequest, preferences, customer);

        await customerRepository.UpdateAsync(customer);
        return new Empty();
    }


    public override async Task<Empty> DeleteCustomerAsync(DeleteCustomerRequest request, ServerCallContext context)
    {
        var id = Guid.TryParse(request.Id, out var guid)
            ? guid
            : throw new ArgumentException("Invalid CustomerId");
        var customer = await customerRepository.GetByIdAsync(id);


        if (customer == null)
            throw new KeyNotFoundException($"Customer {request.Id} not found");

        await customerRepository.DeleteAsync(customer);
        return new Empty();
    }
}