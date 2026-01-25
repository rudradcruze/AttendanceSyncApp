using System;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repository access
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IAttandanceSynchronizationRepository _attandanceSynchronizationRepository;
        private ICompanyRepository _companyRepository;

        public UnitOfWork()
        {
            _context = new AppDbContext();
        }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IAttandanceSynchronizationRepository AttandanceSynchronizations
        {
            get
            {
                if (_attandanceSynchronizationRepository == null)
                {
                    _attandanceSynchronizationRepository = new AttandanceSynchronizationRepository(_context);
                }
                return _attandanceSynchronizationRepository;
            }
        }

        public ICompanyRepository Companies
        {
            get
            {
                if (_companyRepository == null)
                {
                    _companyRepository = new CompanyRepository(_context);
                }
                return _companyRepository;
            }
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
