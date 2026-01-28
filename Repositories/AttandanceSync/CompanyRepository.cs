using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Repository for Company entity operations.
    /// Provides specialized methods for company lookup and batch retrieval
    /// from external synchronization databases.
    /// </summary>
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        /// <summary>
        /// Initializes a new CompanyRepository with the given application context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public CompanyRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves the first company from the database, ordered by ID.
        /// </summary>
        /// <returns>The company with the lowest ID, or null if no companies exist.</returns>
        /// <remarks>
        /// Used when synchronizing with external databases that may have
        /// different company ID schemes.
        /// </remarks>
        public Company GetFirstCompany()
        {
            // Return first company ordered by ID
            return _dbSet
                .OrderBy(c => c.Id)
                .FirstOrDefault();
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
            return _dbSet
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.CompanyName);
        }
    }
}
