using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    /// <summary>
    /// Repository for AttandanceSyncRequest entity operations in the Sync namespace.
    /// Manages user-initiated attendance synchronization requests with support for
    /// filtering, pagination, and comprehensive request tracking with related entities.
    /// </summary>
    public class AttandanceSyncRequestRepository : Repository<AttandanceSyncRequest>, IAttandanceSyncRequestRepository
    {
        /// Reference to the authentication context for request management.
        private readonly AuthDbContext _authContext;

        /// <summary>
        /// Initializes a new AttandanceSyncRequestRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public AttandanceSyncRequestRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        /// <summary>
        /// Retrieves all sync requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter by.</param>
        /// <returns>Collection of sync requests ordered by ID descending (newest first).</returns>
        public IEnumerable<AttandanceSyncRequest> GetByUserId(int userId)
        {
            // Return user's requests ordered by newest first
            return _dbSet.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        /// <summary>
        /// Retrieves all sync requests with related entity details eagerly loaded.
        /// </summary>
        /// <returns>Collection of all requests with user, company, tool, and configuration data.</returns>
        public IEnumerable<AttandanceSyncRequest> GetAllWithDetails()
        {
            // Include all related entities for comprehensive request details
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of sync requests with full details.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of requests with all related entity data.</returns>
        public IEnumerable<AttandanceSyncRequest> GetPaged(int page, int pageSize)
        {
            // Include all related entities and apply pagination
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of sync requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter by.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of user's requests with all related entity data.</returns>
        public IEnumerable<AttandanceSyncRequest> GetPagedByUserId(int userId, int page, int pageSize)
        {
            // Filter by user, include related entities, and apply pagination
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves a single sync request by ID with all related configuration and entity details.
        /// </summary>
        /// <param name="id">The request ID.</param>
        /// <returns>Request with full details, or null if not found.</returns>
        public AttandanceSyncRequest GetWithConfiguration(int id)
        {
            // Include all related entities for complete request information
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .FirstOrDefault(r => r.Id == id);
        }

        /// <summary>
        /// Gets the total count of all sync requests in the system.
        /// </summary>
        /// <returns>Total number of sync requests.</returns>
        public int GetTotalCount()
        {
            return _dbSet.Count();
        }

        /// <summary>
        /// Gets the total count of sync requests for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to count requests for.</param>
        /// <returns>Number of sync requests for the user.</returns>
        public int GetTotalCountByUserId(int userId)
        {
            return _dbSet.Count(r => r.UserId == userId);
        }

        /// <summary>
        /// Retrieves sync requests with advanced filtering options and pagination.
        /// </summary>
        /// <param name="userSearch">Optional search term for user name or email.</param>
        /// <param name="companyId">Optional company ID filter.</param>
        /// <param name="status">Optional status filter (pending/nr, completed/success/cp, failed).</param>
        /// <param name="fromDate">Optional start date filter (inclusive).</param>
        /// <param name="toDate">Optional end date filter (inclusive).</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <param name="totalCount">Output parameter for total count of filtered results.</param>
        /// <returns>Paginated and filtered collection of sync requests with full details.</returns>
        public IEnumerable<AttandanceSyncRequest> GetFiltered(string userSearch, int? companyId, string status, System.DateTime? fromDate, System.DateTime? toDate, int page, int pageSize, out int totalCount)
        {
            // Start with base query including all related entities
            var query = _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .AsQueryable();

            // Apply user search filter if provided
            if (!string.IsNullOrEmpty(userSearch))
            {
                query = query.Where(r => r.User.Name.Contains(userSearch) || r.User.Email.Contains(userSearch));
            }

            // Apply company filter if provided
            if (companyId.HasValue)
            {
                query = query.Where(r => r.CompanyId == companyId.Value);
            }

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                // Status mapping: Pending (null), Completed (true), Failed (false)
                switch (status.ToLower())
                {
                    case "pending":
                    case "nr":
                        query = query.Where(r => r.IsSuccessful == null);
                        break;
                    case "completed":
                    case "success":
                    case "cp":
                        query = query.Where(r => r.IsSuccessful == true);
                        break;
                    case "failed":
                        query = query.Where(r => r.IsSuccessful == false);
                        break;
                }
            }

            // Apply from date filter if provided (inclusive)
            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate.Value);
            }

            // Apply to date filter if provided (inclusive)
            if (toDate.HasValue)
            {
                // Add one day to include the end date fully
                var nextDay = toDate.Value.AddDays(1);
                query = query.Where(r => r.CreatedAt < nextDay);
            }

            // Calculate total count before pagination
            totalCount = query.Count();

            // Apply pagination and return results
            return query.OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
