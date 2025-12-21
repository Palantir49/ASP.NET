using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.DataAccess.Context;

namespace PromoCodeFactory.DataAccess.Repositories.Base;

public class EfRepository<T>(PromoCodeFactoryDbContext promoCodeFactoryDbContext) : IRepository<T> where T : BaseEntity
{
    public IQueryable<T> GetById(Guid id)
    {
        return promoCodeFactoryDbContext.Set<T>().Where(e => e.Id == id);
    }

    public async Task<T> CreateAsync(T entity)
    {
        await promoCodeFactoryDbContext.Set<T>().AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        await Task.FromResult(promoCodeFactoryDbContext.Set<T>().Update(entity));
    }

    public async Task DeleteAsync(T entity)
    {
        await Task.FromResult(promoCodeFactoryDbContext.Set<T>().Remove(entity));
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return promoCodeFactoryDbContext.Set<T>().Where(expression);
    }

    public IQueryable<T> GetAll()
    {
        return promoCodeFactoryDbContext.Set<T>().AsNoTracking();
    }
}