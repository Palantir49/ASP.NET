using System.Threading;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Repositories.Interfaces;

public interface IRepositoryWrapper
{
    ICustomerRepository Customers { get; }
    IPreferenceRepository Preferences { get; }
    IPromoCodeRepository PromoCodes { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}