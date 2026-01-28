using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Generic repository implementation providing basic CRUD operations for all entities.
    /// Serves as the base class for all specific repository implementations,
    /// abstracting Entity Framework data access logic.
    /// </summary>
    /// <typeparam name="T">The entity type managed by this repository.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// Database context for data access.
        protected readonly DbContext _context;

        /// DbSet for the entity type T.
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new repository instance with the given database context.
        /// </summary>
        /// <param name="context">The database context to use for data operations.</param>
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Retrieves an entity by its primary key ID.
        /// </summary>
        /// <param name="id">The primary key ID of the entity.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Retrieves all entities as a queryable collection with no tracking.
        /// </summary>
        /// <returns>IQueryable of all entities (read-only).</returns>
        public IQueryable<T> GetAll()
        {
            // Use AsNoTracking for better performance on read-only queries
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// Finds entities matching the specified predicate condition.
        /// </summary>
        /// <param name="predicate">The filter condition to apply.</param>
        /// <returns>IQueryable of entities matching the predicate.</returns>
        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            // Use AsNoTracking for better performance on read-only queries
            return _dbSet.AsNoTracking().Where(predicate);
        }

        /// <summary>
        /// Retrieves the first entity matching the predicate, or null if none found.
        /// </summary>
        /// <param name="predicate">The filter condition to apply.</param>
        /// <returns>First matching entity or null.</returns>
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Adds a new entity to the database context.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <remarks>Call SaveChanges on the unit of work to persist the change.</remarks>
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        /// <summary>
        /// Marks an entity as modified in the database context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <remarks>Call SaveChanges on the unit of work to persist the change.</remarks>
        public void Update(T entity)
        {
            // Get the entity's entry in the context
            var entry = _context.Entry(entity);

            // If entity is not being tracked, attach it first
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }

            // Mark the entity as modified
            entry.State = EntityState.Modified;
        }

        /// <summary>
        /// Marks an entity for deletion from the database.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <remarks>Call SaveChanges on the unit of work to persist the change.</remarks>
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Gets the total count of all entities in the table.
        /// </summary>
        /// <returns>Total number of entities.</returns>
        public int Count()
        {
            return _dbSet.Count();
        }

        /// <summary>
        /// Gets the count of entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">The filter condition to apply.</param>
        /// <returns>Number of entities matching the predicate.</returns>
        public int Count(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }
    }
}
