using System;
using AttandanceSyncApp.Repositories.Interfaces.Auth;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    public interface IAuthUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ILoginSessionRepository LoginSessions { get; }
        ISyncCompanyRepository SyncCompanies { get; }
        IToolRepository Tools { get; }
        IAttandanceSyncRequestRepository AttandanceSyncRequests { get; }
        IDatabaseConfigurationRepository DatabaseConfigurations { get; }
        int SaveChanges();
    }
}
