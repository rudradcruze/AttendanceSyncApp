using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    /// <summary>
    /// Repository for CompanyRequest entity operations.
    /// Manages company access requests from users who want to connect
    /// their organization's database for attendance synchronization.
    /// </summary>
    public class CompanyRequestRepository : Repository<CompanyRequest>, ICompanyRequestRepository
    {
        /// Reference to the authentication context for request management.
        private readonly AuthDbContext _authContext;

        /// <summary>
        /// Initializes a new CompanyRequestRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public CompanyRequestRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        /// <summary>
        /// Retrieves all company requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter by.</param>
        /// <returns>Collection of company requests ordered by ID descending (newest first).</returns>
        public IEnumerable<CompanyRequest> GetByUserId(int userId)
        {
            // Return user's requests ordered by newest first
            return _dbSet.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        /// <summary>
        /// Retrieves all company requests with related entity details eagerly loaded.
        /// </summary>
        /// <returns>Collection of all requests with user, employee, company, and tool data.</returns>
        public IEnumerable<CompanyRequest> GetAllWithDetails()
        {
            // Include all related entities for comprehensive request details
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of company requests with full details.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of requests with all related entity data.</returns>
        public IEnumerable<CompanyRequest> GetPaged(int page, int pageSize)
        {
            // Include all related entities and apply pagination
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of company requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter by.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of user's requests with all related entity data.</returns>
        public IEnumerable<CompanyRequest> GetPagedByUserId(int userId, int page, int pageSize)
        {
            // Filter by user, include related entities, and apply pagination
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves a single company request by ID with all related entity details.
        /// </summary>
        /// <param name="id">The request ID.</param>
        /// <returns>Request with full details, or null if not found.</returns>
        public CompanyRequest GetWithDetails(int id)
        {
            // Include all related entities for complete request information
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .FirstOrDefault(r => r.Id == id);
        }

        /// <summary>
        /// Gets the total count of all company requests in the system.
        /// </summary>
        /// <returns>Total number of company requests.</returns>
        public int GetTotalCount()
        {
            return _dbSet.Count();
        }

        /// <summary>
        /// Gets the total count of company requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to count requests for.</param>
        /// <returns>Number of company requests for the user.</returns>
        public int GetTotalCountByUserId(int userId)
        {
            return _dbSet.Count(r => r.UserId == userId);
        }
    }
}
