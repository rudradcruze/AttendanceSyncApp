using System;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work interface for managing transactions and repository access
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IAttandanceSynchronizationRepository AttandanceSynchronizations { get; }
        ICompanyRepository Companies { get; }
        int SaveChanges();
    }
}
