using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.SalaryGarbge
{
    /// <summary>
    /// Repository for DatabaseAccess entity operations.
    /// Manages database access records for the salary garbage collection module,
    /// tracking which databases on specific servers are accessible for cleanup operations.
    /// </summary>
    public class DatabaseAccessRepository : Repository<DatabaseAccess>, IDatabaseAccessRepository
    {
        /// <summary>
        /// Initializes a new DatabaseAccessRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public DatabaseAccessRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all active database access records for a specific server.
        /// </summary>
        /// <param name="serverIpId">The server IP ID to retrieve database access records for.</param>
        /// <returns>Collection of active database access records ordered by database name.</returns>
        public IEnumerable<DatabaseAccess> GetByServerIpId(int serverIpId)
        {
            // Filter by server and active status
            return _dbSet.AsNoTracking()
                .Where(da => da.ServerIpId == serverIpId && da.IsActive)
                .OrderBy(da => da.DatabaseName)
                .ToList();
        }

        /// <summary>
        /// Retrieves a specific database access record by server and database name.
        /// </summary>
        /// <param name="serverIpId">The server IP ID.</param>
        /// <param name="databaseName">The database name.</param>
        /// <returns>Database access record if found and active, otherwise null.</returns>
        public DatabaseAccess GetByServerIpAndDatabase(int serverIpId, string databaseName)
        {
            // Find active database access record for server-database combination
            return _dbSet.AsNoTracking()
                .FirstOrDefault(da => da.ServerIpId == serverIpId
                    && da.DatabaseName == databaseName
                    && da.IsActive);
        }

        /// <summary>
        /// Checks if an active database access record exists for the specified server and database.
        /// </summary>
        /// <param name="serverIpId">The server IP ID to check.</param>
        /// <param name="databaseName">The database name to check.</param>
        /// <returns>True if an active access record exists, false otherwise.</returns>
        public bool DatabaseAccessExists(int serverIpId, string databaseName)
        {
            return _dbSet.Any(da => da.ServerIpId == serverIpId
                && da.DatabaseName == databaseName
                && da.IsActive);
        }

        /// <summary>
        /// Retrieves all databases on a server that have been granted access permission.
        /// </summary>
        /// <param name="serverIpId">The server IP ID to retrieve accessible databases for.</param>
        /// <returns>Collection of accessible database records ordered by database name.</returns>
        public IEnumerable<DatabaseAccess> GetAccessibleDatabasesByServerId(int serverIpId)
        {
            // Filter by server, active status, and access permission
            return _dbSet.AsNoTracking()
                .Where(da => da.ServerIpId == serverIpId
                    && da.IsActive
                    && da.HasAccess)
                .OrderBy(da => da.DatabaseName)
                .ToList();
        }
    }
}
