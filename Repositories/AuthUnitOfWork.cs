using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Repositories.Interfaces.Auth;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;
using AttandanceSyncApp.Repositories.Auth;
using AttandanceSyncApp.Repositories.AttandanceSync;
using AttandanceSyncApp.Repositories.SalaryGarbge;

namespace AttandanceSyncApp.Repositories
{
    public class AuthUnitOfWork : IAuthUnitOfWork
    {
        private readonly AuthDbContext _context;

        private IUserRepository _userRepository;
        private ILoginSessionRepository _loginSessionRepository;
        private ISyncCompanyRepository _syncCompanyRepository;
        private IToolRepository _toolRepository;
        private IEmployeeRepository _employeeRepository;
        private IAttandanceSyncRequestRepository _syncRequestRepository;
        private ICompanyRequestRepository _companyRequestRepository;
        private IDatabaseConfigurationRepository _dbConfigRepository;
        private IDatabaseAssignRepository _databaseAssignRepository;
        private IUserToolRepository _userToolRepository;

        // SalaryGarbge repositories
        private IServerIpRepository _serverIpRepository;
        private IDatabaseAccessRepository _databaseAccessRepository;

        public AuthUnitOfWork()
        {
            _context = new AuthDbContext();
        }

        public AuthUnitOfWork(AuthDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users =>
            _userRepository ?? (_userRepository = new UserRepository(_context));

        public ILoginSessionRepository LoginSessions =>
            _loginSessionRepository ?? (_loginSessionRepository = new LoginSessionRepository(_context));

        public ISyncCompanyRepository SyncCompanies =>
            _syncCompanyRepository ?? (_syncCompanyRepository = new SyncCompanyRepository(_context));

        public IToolRepository Tools =>
            _toolRepository ?? (_toolRepository = new ToolRepository(_context));

        public IEmployeeRepository Employees =>
            _employeeRepository ?? (_employeeRepository = new EmployeeRepository(_context));

        public IAttandanceSyncRequestRepository AttandanceSyncRequests =>
            _syncRequestRepository ?? (_syncRequestRepository = new AttandanceSyncRequestRepository(_context));

        public ICompanyRequestRepository CompanyRequests =>
            _companyRequestRepository ?? (_companyRequestRepository = new CompanyRequestRepository(_context));

        public IDatabaseConfigurationRepository DatabaseConfigurations =>
            _dbConfigRepository ?? (_dbConfigRepository = new DatabaseConfigurationRepository(_context));

        public IDatabaseAssignRepository DatabaseAssignments =>
            _databaseAssignRepository ?? (_databaseAssignRepository = new DatabaseAssignRepository(_context));

        public IUserToolRepository UserTools =>
            _userToolRepository ?? (_userToolRepository = new UserToolRepository(_context));

        // SalaryGarbge repositories
        public IServerIpRepository ServerIps =>
            _serverIpRepository ?? (_serverIpRepository = new ServerIpRepository(_context));

        public IDatabaseAccessRepository DatabaseAccess =>
            _databaseAccessRepository ?? (_databaseAccessRepository = new DatabaseAccessRepository(_context));

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
