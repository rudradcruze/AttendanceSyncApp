using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    /// <summary>
    /// Repository for DatabaseAssign entity operations in the Sync namespace.
    /// Manages database configuration assignments to company requests,
    /// linking user requests with appropriate database connection settings.
    /// </summary>
    public class DatabaseAssignRepository : Repository<DatabaseAssign>, IDatabaseAssignRepository
    {
        /// Reference to the authentication context for assignment management.
        private readonly AuthDbContext _authContext;

        /// <summary>
        /// Initializes a new DatabaseAssignRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public DatabaseAssignRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        /// <summary>
        /// Retrieves all database assignments with related entity details eagerly loaded.
        /// </summary>
        /// <returns>Collection of all assignments with company request, user, employee, company, tool, and configuration data.</returns>
        public IEnumerable<DatabaseAssign> GetAllWithDetails()
        {
            // Include all related entities for comprehensive assignment details
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .OrderByDescending(da => da.Id)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of database assignments with full details.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of assignments with all related entity data.</returns>
        public IEnumerable<DatabaseAssign> GetPaged(int page, int pageSize)
        {
            // Include all related entities and apply pagination
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .OrderByDescending(da => da.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves a single database assignment by ID with all related entity details.
        /// </summary>
        /// <param name="id">The assignment ID.</param>
        /// <returns>Assignment with full details, or null if not found.</returns>
        public DatabaseAssign GetWithDetails(int id)
        {
            // Include all related entities for complete assignment information
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .FirstOrDefault(da => da.Id == id);
        }

        /// <summary>
        /// Retrieves the database assignment for a specific company request.
        /// </summary>
        /// <param name="companyRequestId">The company request ID to find assignment for.</param>
        /// <returns>Database assignment with configuration details, or null if not found.</returns>
        public DatabaseAssign GetByCompanyRequestId(int companyRequestId)
        {
            // Include database configuration for connection information
            return _dbSet.AsNoTracking()
                .Include(da => da.DatabaseConfiguration)
                .FirstOrDefault(da => da.CompanyRequestId == companyRequestId);
        }

        /// <summary>
        /// Gets the total count of all database assignments in the system.
        /// </summary>
        /// <returns>Total number of database assignments.</returns>
        public int GetTotalCount()
        {
            return _dbSet.Count();
        }

        /// <summary>
        /// Checks if a database assignment exists for the specified company request.
        /// </summary>
        /// <param name="companyRequestId">The company request ID to check.</param>
        /// <returns>True if an assignment exists, false otherwise.</returns>
        public bool HasAssignment(int companyRequestId)
        {
            return _dbSet.Any(da => da.CompanyRequestId == companyRequestId);
        }
    }
}
