using NganHangDeThi.Common.Interfaces;
using System.Linq.Expressions;

namespace NganHangDeThi.Repository.GenericRepos;

// Qui định T phải là 1 class và phải kế thừa IEntity (tức phải có trường Id).
public interface IGenericRepo<T, TKey> where T : class, IEntity<TKey>
{
    List<T> GetAll(
        Expression<Func<T, bool>>? filter = null,
        bool asNoTracking = true,
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null
    );

    T? GetById(
        TKey id, 
        bool asNoTracking = true,
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null
    );
    
    void Add(T entity);
    void AddMultiple(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    void DeleteMultiple(IEnumerable<T> entities);

    int Count(Expression<Func<T, bool>>? filter = null);
}
