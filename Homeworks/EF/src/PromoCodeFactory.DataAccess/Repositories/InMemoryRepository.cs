using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories;

public class InMemoryRepository<T>
    : IRepository<T>
    where T : BaseEntity
{
    public InMemoryRepository(IEnumerable<T> data)
    {
        Data = data;
    }

    protected IEnumerable<T> Data { get; set; }

    public IQueryable<T> GetById(Guid id)
    {
        return (IQueryable<T>)Data.Where(x => x.Id == id);
    }

    public Task<T> CreateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> GetAll()
    {
        return Data.AsQueryable();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }
}