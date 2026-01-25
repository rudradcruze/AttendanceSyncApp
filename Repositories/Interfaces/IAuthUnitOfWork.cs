using System;
using AttandanceSyncApp.Repositories.Interfaces.Auth;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    public interface IAuthUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ILoginSessionRepository LoginSessions { get; }
        ISyncCompanyRepository SyncCompanies { get; }
        IToolRepository Tools { get; }
        IEmployeeRepository Employees { get; }
        IAttandanceSyncRequestRepository AttandanceSyncRequests { get; }
        ICompanyRequestRepository CompanyRequests { get; }
        IDatabaseConfigurationRepository DatabaseConfigurations { get; }
        IDatabaseAssignRepository DatabaseAssignments { get; }
        IUserToolRepository UserTools { get; }

        // SalaryGarbge repositories
        IServerIpRepository ServerIps { get; }
        IDatabaseAccessRepository DatabaseAccess { get; }

        int SaveChanges();
    }
}
