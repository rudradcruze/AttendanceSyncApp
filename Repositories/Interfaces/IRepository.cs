using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        T GetById(int id);
        IQueryable<T> GetAll();
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        int Count();
        int Count(Expression<Func<T, bool>> predicate);
    }
}
