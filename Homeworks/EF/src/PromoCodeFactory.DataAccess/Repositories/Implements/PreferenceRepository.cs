using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Context;
using PromoCodeFactory.DataAccess.Repositories.Base;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;

namespace PromoCodeFactory.DataAccess.Repositories.Implements;

public class PreferenceRepository(PromoCodeFactoryDbContext promoCodeFactoryDbContext)
    : EfRepository<Preference>(promoCodeFactoryDbContext), IPreferenceRepository
{
}