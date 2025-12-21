using System.Threading;
using System.Threading.Tasks;
using PromoCodeFactory.DataAccess.Context;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;

namespace PromoCodeFactory.DataAccess.Repositories.Implements;

public class RepositoryWrapper(PromoCodeFactoryDbContext promoCodeFactoryDbContext) : IRepositoryWrapper
{
    private ICustomerRepository _customers;
    private IPreferenceRepository _preferences;
    private IPromoCodeRepository _promoCodes;

    public ICustomerRepository Customers => _customers ??= new CustomerRepository(promoCodeFactoryDbContext);

    public IPreferenceRepository Preferences => _preferences ??= new PreferenceRepository(promoCodeFactoryDbContext);

    public IPromoCodeRepository PromoCodes => _promoCodes ??= new PromoCodeRepository(promoCodeFactoryDbContext);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await promoCodeFactoryDbContext.SaveChangesAsync(cancellationToken);
    }
}