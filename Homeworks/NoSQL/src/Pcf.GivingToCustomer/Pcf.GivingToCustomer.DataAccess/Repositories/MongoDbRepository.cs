using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.DataAccess.Repositories;

public class MongoDbRepository<T>(MongoDbDataContext context) : IRepository<T>
    where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection = GetCollection(context);
    

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _collection.Find(Builders<T>.Filter.Eq(x => x.Id, id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids)
    {
        return await _collection.Find(Builders<T>.Filter.In(x => x.Id, ids)).ToListAsync();
    }

    public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
       await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await _collection.DeleteOneAsync(x => x.Id == entity.Id);
    }

    private static IMongoCollection<T> GetCollection(MongoDbDataContext context)
    {
        var type = typeof(T);

        if (type == typeof(Customer))
            return (IMongoCollection<T>)context.Customers;
        if (type == typeof(Preference))
            return (IMongoCollection<T>)context.Preferences;
        if (type == typeof(PromoCode))
            return (IMongoCollection<T>)context.PromoCodes;

        throw new InvalidOperationException($"Unknown collection for type {type.Name}");
    }
}