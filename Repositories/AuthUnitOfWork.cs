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
    /// <summary>
    /// Unit of Work implementation for the authentication and authorization database.
    /// Provides centralized access to all repositories in the AuthDbContext and manages
    /// transactional consistency across user authentication, sessions, companies, and tools.
    /// </summary>
    public class AuthUnitOfWork : IAuthUnitOfWork
    {
        /// Authentication database context.
        private readonly AuthDbContext _context;

        /// Lazy-initialized repository for user entities.
        private IUserRepository _userRepository;

        /// Lazy-initialized repository for login session tracking.
        private ILoginSessionRepository _loginSessionRepository;

        /// Lazy-initialized repository for synchronized companies.
        private ISyncCompanyRepository _syncCompanyRepository;

        /// Lazy-initialized repository for tools available to users.
        private IToolRepository _toolRepository;

        /// Lazy-initialized repository for employee records.
        private IEmployeeRepository _employeeRepository;

        /// Lazy-initialized repository for attendance sync requests.
        private IAttandanceSyncRequestRepository _syncRequestRepository;

        /// Lazy-initialized repository for company requests.
        private ICompanyRequestRepository _companyRequestRepository;

        /// Lazy-initialized repository for database configurations.
        private IDatabaseConfigurationRepository _dbConfigRepository;

        /// Lazy-initialized repository for database assignments.
        private IDatabaseAssignRepository _databaseAssignRepository;

        /// Lazy-initialized repository for user-tool assignments.
        private IUserToolRepository _userToolRepository;

        /// Lazy-initialized repository for salary garbage server IPs.
        private IServerIpRepository _serverIpRepository;

        /// Lazy-initialized repository for database access records.
        private IDatabaseAccessRepository _databaseAccessRepository;

        /// <summary>
        /// Initializes a new Unit of Work with a default AuthDbContext.
        /// </summary>
        public AuthUnitOfWork()
        {
            _context = new AuthDbContext();
        }

        /// <summary>
        /// Initializes a new Unit of Work with a provided AuthDbContext.
        /// </summary>
        /// <param name="context">The authentication database context to use.</param>
        public AuthUnitOfWork(AuthDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the repository for User entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IUserRepository Users =>
            _userRepository ?? (_userRepository = new UserRepository(_context));

        /// <summary>
        /// Gets the repository for LoginSession entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public ILoginSessionRepository LoginSessions =>
            _loginSessionRepository ?? (_loginSessionRepository = new LoginSessionRepository(_context));

        /// <summary>
        /// Gets the repository for SyncCompany entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public ISyncCompanyRepository SyncCompanies =>
            _syncCompanyRepository ?? (_syncCompanyRepository = new SyncCompanyRepository(_context));

        /// <summary>
        /// Gets the repository for Tool entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IToolRepository Tools =>
            _toolRepository ?? (_toolRepository = new ToolRepository(_context));

        /// <summary>
        /// Gets the repository for Employee entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IEmployeeRepository Employees =>
            _employeeRepository ?? (_employeeRepository = new EmployeeRepository(_context));

        /// <summary>
        /// Gets the repository for AttandanceSyncRequest entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IAttandanceSyncRequestRepository AttandanceSyncRequests =>
            _syncRequestRepository ?? (_syncRequestRepository = new AttandanceSyncRequestRepository(_context));

        /// <summary>
        /// Gets the repository for CompanyRequest entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public ICompanyRequestRepository CompanyRequests =>
            _companyRequestRepository ?? (_companyRequestRepository = new CompanyRequestRepository(_context));

        /// <summary>
        /// Gets the repository for DatabaseConfiguration entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IDatabaseConfigurationRepository DatabaseConfigurations =>
            _dbConfigRepository ?? (_dbConfigRepository = new DatabaseConfigurationRepository(_context));

        /// <summary>
        /// Gets the repository for DatabaseAssign entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IDatabaseAssignRepository DatabaseAssignments =>
            _databaseAssignRepository ?? (_databaseAssignRepository = new DatabaseAssignRepository(_context));

        /// <summary>
        /// Gets the repository for UserTool entities.
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IUserToolRepository UserTools =>
            _userToolRepository ?? (_userToolRepository = new UserToolRepository(_context));

        /// <summary>
        /// Gets the repository for ServerIp entities (SalaryGarbge module).
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IServerIpRepository ServerIps =>
            _serverIpRepository ?? (_serverIpRepository = new ServerIpRepository(_context));

        /// <summary>
        /// Gets the repository for DatabaseAccess entities (SalaryGarbge module).
        /// </summary>
        /// <remarks>Repository is created lazily on first access using null-coalescing.</remarks>
        public IDatabaseAccessRepository DatabaseAccess =>
            _databaseAccessRepository ?? (_databaseAccessRepository = new DatabaseAccessRepository(_context));

        /// <summary>
        /// Commits all pending changes to the authentication database.
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
