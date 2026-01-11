using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Repository implementation for AttandanceSynchronization entity
    /// </summary>
    public class AttandanceSynchronizationRepository : Repository<AttandanceSynchronization>, IAttandanceSynchronizationRepository
    {
        public AttandanceSynchronizationRepository(AppDbContext context) : base(context)
        {
        }

        public IEnumerable<AttandanceSynchronization> GetPaged(int page, int pageSize)
        {
            return _dbSet
                .AsNoTracking()
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public IEnumerable<AttandanceSynchronization> GetByIds(int[] ids)
        {
            return _dbSet
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToList();
        }
    }
}
