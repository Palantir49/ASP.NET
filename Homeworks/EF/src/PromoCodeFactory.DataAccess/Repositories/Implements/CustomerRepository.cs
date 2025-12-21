using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Context;
using PromoCodeFactory.DataAccess.Repositories.Base;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;

namespace PromoCodeFactory.DataAccess.Repositories.Implements;

public class CustomerRepository(PromoCodeFactoryDbContext promoCodeFactoryDbContext)
    : EfRepository<Customer>(promoCodeFactoryDbContext), ICustomerRepository
{
}