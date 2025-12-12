using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories;

public class InMemoryRepository<T>(IEnumerable<T> data) : IRepository<T>
    where T : BaseEntity
{
    private IEnumerable<T> Data { get; set; } = data;

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(Data);
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }

    public Task AddAsync(T entity)
    {
        var materializedData = Data.ToList();
        materializedData.Add(entity);
        Data = materializedData;
        return Task.FromResult(Data);
    }

    public Task UpdateAsync(T entity)
    {
        var materializedData = Data.ToList();
        var index = materializedData.FindIndex(x => x.Id == entity.Id);
        materializedData[index] = entity;
        Data = materializedData;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var materializedData = Data.ToList();
        materializedData.RemoveAll(x => x.Id == id);
        Data = materializedData;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return Task.FromResult(Data.Where(predicate.Compile()));
    }
}