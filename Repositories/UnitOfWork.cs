using System;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repository access.
    /// Provides centralized access to repositories and ensures transactional consistency
    /// across multiple repository operations within the main application database.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// Main application database context.
        private readonly AppDbContext _context;

        /// Lazy-initialized repository for attendance synchronization records.
        private IAttandanceSynchronizationRepository _attandanceSynchronizationRepository;

        /// Lazy-initialized repository for company entities.
        private ICompanyRepository _companyRepository;

        /// <summary>
        /// Initializes a new Unit of Work with a default AppDbContext.
        /// </summary>
        public UnitOfWork()
        {
            _context = new AppDbContext();
        }

        /// <summary>
        /// Initializes a new Unit of Work with a provided AppDbContext.
        /// </summary>
        /// <param name="context">The database context to use.</param>
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the repository for AttandanceSynchronization entities.
        /// </summary>
        /// <remarks>
        /// Repository is created lazily on first access and reused thereafter.
        /// </remarks>
        public IAttandanceSynchronizationRepository AttandanceSynchronizations
        {
            get
            {
                // Lazy initialization pattern
                if (_attandanceSynchronizationRepository == null)
                {
                    _attandanceSynchronizationRepository = new AttandanceSynchronizationRepository(_context);
                }
                return _attandanceSynchronizationRepository;
            }
        }

        /// <summary>
        /// Gets the repository for Company entities.
        /// </summary>
        /// <remarks>
        /// Repository is created lazily on first access and reused thereafter.
        /// </remarks>
        public ICompanyRepository Companies
        {
            get
            {
                // Lazy initialization pattern
                if (_companyRepository == null)
                {
                    _companyRepository = new CompanyRepository(_context);
                }
                return _companyRepository;
            }
        }

        /// <summary>
        /// Commits all pending changes to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Releases all database resources used by this unit of work.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
