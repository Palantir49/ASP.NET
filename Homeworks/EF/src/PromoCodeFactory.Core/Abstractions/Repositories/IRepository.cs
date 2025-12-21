using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.Core.Abstractions.Repositories;

public interface IRepository<T>
    where T : BaseEntity
{
    IQueryable<T> GetAll();

    IQueryable<T> GetById(Guid id);

    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);

    Task<T> CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);
}