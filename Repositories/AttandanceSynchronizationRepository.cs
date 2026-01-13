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

        public IEnumerable<AttandanceSynchronization> GetPaged(int page, int pageSize, string sortColumn, string sortDirection)
        {
            IQueryable<AttandanceSynchronization> query =
                _dbSet.AsNoTracking();

            switch (sortColumn)
            {
                case "Id":
                    query = sortDirection == "ASC"
                        ? query.OrderBy(x => x.Id)
                        : query.OrderByDescending(x => x.Id);
                    break;

                case "FromDate":
                    query = sortDirection == "ASC"
                        ? query.OrderBy(x => x.FromDate)
                        : query.OrderByDescending(x => x.FromDate);
                    break;

                case "ToDate":
                default:
                    query = sortDirection == "ASC"
                        ? query.OrderBy(x => x.ToDate)
                        : query.OrderByDescending(x => x.ToDate);
                    break;
            }

            return query
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
