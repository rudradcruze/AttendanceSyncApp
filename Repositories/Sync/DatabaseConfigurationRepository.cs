using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    public class DatabaseConfigurationRepository : Repository<DatabaseConfiguration>, IDatabaseConfigurationRepository
    {
        public DatabaseConfigurationRepository(AuthDbContext context) : base(context)
        {
        }

        public DatabaseConfiguration GetByRequestId(int requestId)
        {
            return _dbSet.AsNoTracking()
                .Include(dc => dc.Request)
                .Include(dc => dc.AssignedByUser)
                .FirstOrDefault(dc => dc.RequestId == requestId);
        }

        public bool HasConfiguration(int requestId)
        {
            return _dbSet.Any(dc => dc.RequestId == requestId);
        }
    }
}
