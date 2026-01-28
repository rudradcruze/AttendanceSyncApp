using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    /// <summary>
    /// Repository for DatabaseConfiguration entity operations in the Sync namespace.
    /// Manages external database connection configurations used for
    /// attendance synchronization with client company databases.
    /// </summary>
    public class DatabaseConfigurationRepository : Repository<DatabaseConfiguration>, IDatabaseConfigurationRepository
    {
        /// <summary>
        /// Initializes a new DatabaseConfigurationRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public DatabaseConfigurationRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves the database configuration for a specific company.
        /// </summary>
        /// <param name="companyId">The company ID to retrieve configuration for.</param>
        /// <returns>Database configuration for the company, or null if not configured.</returns>
        public DatabaseConfiguration GetByCompanyId(int companyId)
        {
            // Use AsNoTracking for read-only query performance
            return _dbSet.AsNoTracking()
                //.Include(dc => dc.Company) // SyncCompany - include if navigation needed
                .FirstOrDefault(dc => dc.CompanyId == companyId);
        }

        /// <summary>
        /// Checks if a database configuration exists for the specified company.
        /// </summary>
        /// <param name="companyId">The company ID to check.</param>
        /// <returns>True if configuration exists, false otherwise.</returns>
        public bool HasConfiguration(int companyId)
        {
            return _dbSet.Any(dc => dc.CompanyId == companyId);
        }
    }
}
