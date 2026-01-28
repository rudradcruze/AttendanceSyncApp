using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    /// <summary>
    /// Repository for SyncCompany entity operations in the Sync namespace.
    /// Manages companies configured for attendance synchronization,
    /// tracking company status and providing lookup functionality.
    /// </summary>
    public class SyncCompanyRepository : Repository<SyncCompany>, ISyncCompanyRepository
    {
        /// <summary>
        /// Initializes a new SyncCompanyRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public SyncCompanyRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all companies with "Active" status.
        /// </summary>
        /// <returns>Collection of active companies ordered alphabetically by name.</returns>
        public IEnumerable<SyncCompany> GetActiveCompanies()
        {
            // Filter by Active status and order by name
            return _dbSet.AsNoTracking()
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves company names for a collection of company IDs.
        /// </summary>
        /// <param name="companyIds">List of company IDs to retrieve names for.</param>
        /// <returns>Dictionary mapping company ID to company name.</returns>
        public Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds)
        {
            // Use AsNoTracking for read-only performance
            // Return dictionary for efficient lookup
            return _dbSet.AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.Name);
        }
    }
}
