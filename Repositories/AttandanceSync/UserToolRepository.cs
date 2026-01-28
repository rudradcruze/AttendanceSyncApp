using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    /// <summary>
    /// Repository for UserTool entity operations.
    /// Manages tool assignments to users, tracking which users have access to which
    /// attendance synchronization tools and their assignment/revocation status.
    /// </summary>
    public class UserToolRepository : Repository<UserTool>, IUserToolRepository
    {
        /// <summary>
        /// Initializes a new UserToolRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public UserToolRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all active tool assignments for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to retrieve tool assignments for.</param>
        /// <returns>Collection of active, non-revoked tool assignments with tool, user, and assigner details.</returns>
        public IEnumerable<UserTool> GetActiveToolsByUserId(int userId)
        {
            // Filter by user, non-revoked status, and active tools only
            return _dbSet.AsNoTracking()
                .Include(ut => ut.Tool)
                .Include(ut => ut.User)
                .Include(ut => ut.AssignedByUser)
                .Where(ut => ut.UserId == userId && !ut.IsRevoked && ut.Tool.IsActive)
                .OrderBy(ut => ut.Tool.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves a paginated subset of all tool assignments in the system.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of tool assignments with user, tool, and assigner details.</returns>
        public IEnumerable<UserTool> GetAllAssignments(int page, int pageSize)
        {
            // Include related entities and apply pagination
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Include(ut => ut.Tool)
                .Include(ut => ut.AssignedByUser)
                .OrderByDescending(ut => ut.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Gets the total count of all tool assignments in the system.
        /// </summary>
        /// <returns>Total number of tool assignments (including revoked).</returns>
        public int GetTotalAssignmentsCount()
        {
            return _dbSet.Count();
        }

        /// <summary>
        /// Retrieves a specific active tool assignment for a user-tool pair.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="toolId">The tool ID.</param>
        /// <returns>Active tool assignment, or null if not found or revoked.</returns>
        public UserTool GetActiveAssignment(int userId, int toolId)
        {
            // Find non-revoked assignment for user-tool combination
            return _dbSet.FirstOrDefault(ut =>
                ut.UserId == userId &&
                ut.ToolId == toolId &&
                !ut.IsRevoked);
        }

        /// <summary>
        /// Checks if an active tool assignment exists for a user-tool pair.
        /// </summary>
        /// <param name="userId">The user ID to check.</param>
        /// <param name="toolId">The tool ID to check.</param>
        /// <returns>True if an active assignment exists, false otherwise.</returns>
        public bool HasActiveAssignment(int userId, int toolId)
        {
            return _dbSet.Any(ut =>
                ut.UserId == userId &&
                ut.ToolId == toolId &&
                !ut.IsRevoked);
        }

        /// <summary>
        /// Retrieves all active tool assignments for a specific tool.
        /// </summary>
        /// <param name="toolId">The tool ID to retrieve assignments for.</param>
        /// <returns>Collection of active assignments with user details, ordered by user name.</returns>
        public IEnumerable<UserTool> GetAssignmentsByToolId(int toolId)
        {
            // Filter by tool and non-revoked status
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Where(ut => ut.ToolId == toolId && !ut.IsRevoked)
                .OrderBy(ut => ut.User.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves a single tool assignment by ID with all related entity details.
        /// </summary>
        /// <param name="id">The assignment ID.</param>
        /// <returns>Tool assignment with full details, or null if not found.</returns>
        public UserTool GetWithDetails(int id)
        {
            // Include all related entities for complete assignment information
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Include(ut => ut.Tool)
                .Include(ut => ut.AssignedByUser)
                .FirstOrDefault(ut => ut.Id == id);
        }
    }
}
