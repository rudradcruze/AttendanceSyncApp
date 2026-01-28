using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    /// <summary>
    /// Repository for Tool entity operations in the Sync namespace.
    /// Manages attendance synchronization tools (e.g., DataSoft HRM, other HR systems),
    /// providing tool filtering and batch retrieval capabilities.
    /// </summary>
    public class ToolRepository : Repository<Tool>, IToolRepository
    {
        /// <summary>
        /// Initializes a new ToolRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public ToolRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all active tools available for attendance synchronization.
        /// </summary>
        /// <returns>Collection of active tools ordered alphabetically by name.</returns>
        public IEnumerable<Tool> GetActiveTools()
        {
            // Filter by IsActive flag and order by name
            return _dbSet.AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves tool names for a collection of tool IDs.
        /// </summary>
        /// <param name="toolIds">List of tool IDs to retrieve names for.</param>
        /// <returns>Dictionary mapping tool ID to tool name.</returns>
        public Dictionary<int, string> GetToolNamesByIds(List<int> toolIds)
        {
            // Use AsNoTracking for read-only performance
            // Return dictionary for efficient lookup
            return _dbSet.AsNoTracking()
                .Where(t => toolIds.Contains(t.Id))
                .ToDictionary(t => t.Id, t => t.Name);
        }
    }
}
