using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    /// <summary>
    /// Repository for AttandanceSynchronization entity operations.
    /// Manages attendance synchronization records in external company databases,
    /// tracking sync requests and their processing status.
    /// </summary>
    public class AttandanceSynchronizationRepository : Repository<AttandanceSynchronization>, IAttandanceSynchronizationRepository
    {
        /// <summary>
        /// Initializes a new AttandanceSynchronizationRepository with the given application context.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public AttandanceSynchronizationRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves a paginated subset of attendance synchronization records.
        /// </summary>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>Paginated collection of synchronization records ordered by ID descending.</returns>
        public IEnumerable<AttandanceSynchronization> GetPaged(int page, int pageSize)
        {
            // Return paginated results ordered by newest first
            return _dbSet
                .AsNoTracking()
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Retrieves multiple synchronization records by their IDs.
        /// </summary>
        /// <param name="ids">Array of synchronization IDs to retrieve.</param>
        /// <returns>Collection of synchronization records matching the provided IDs.</returns>
        public IEnumerable<AttandanceSynchronization> GetByIds(int[] ids)
        {
            // Filter by provided IDs using Contains
            return _dbSet
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToList();
        }
    }
}
