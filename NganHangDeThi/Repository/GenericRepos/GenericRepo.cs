using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Common.Interfaces;
using NganHangDeThi.Data.DataContext;
using System.Linq.Expressions;

namespace NganHangDeThi.Repository.GenericRepos;

public class GenericRepo<T, TKey>(AppDbContext context) : IGenericRepo<T, TKey> where T : class, IEntity<TKey>
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<T> _table = context.Set<T>();
    
    public virtual List<T> GetAll(
        Expression<Func<T, bool>>? filter = null, 
        bool asNoTracking = true, 
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null)
    {
        var query = _table.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includeFunc != null)
        {
            query = includeFunc(query);
        }

        return [.. query];
    }

    public T? GetById(
        TKey id, 
        bool asNoTracking = true, 
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null)
    {
        var keyProperty = _context.Model
                                .FindEntityType(typeof(T))!
                                .FindPrimaryKey()!
                                .Properties[0];

        var parameter = Expression.Parameter(typeof(T), "e");
        var memberAccess = Expression.Property(parameter, keyProperty.Name);
        var constant = Expression.Constant(id);
        var predicate = Expression.Equal(memberAccess, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);

        var query = _table.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (includeFunc != null)
        {
            query = includeFunc(query);
        }

        return query.FirstOrDefault(lambda);
    }

    public void Add(T entity)
    {
        _table.Add(entity);
    }

    public void AddMultiple(IEnumerable<T> entities)
    {
        _table.AddRange(entities);
    }

    public void Update(T entity)
    {
        _table.Update(entity);
    }

    public void Delete(T entity)
    {
        _table.Remove(entity);
    }

    public void DeleteMultiple(IEnumerable<T> entities)
    {
        _table.RemoveRange(entities);
    }

    public int Count(Expression<Func<T, bool>>? filter = null)
    {
        return filter == null ? 
                _table.AsNoTracking().Count() :
                _table.AsNoTracking().Count(filter);
    }
}
