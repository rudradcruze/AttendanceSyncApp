using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.SalaryGarbge
{
    public class DatabaseAccessRepository : Repository<DatabaseAccess>, IDatabaseAccessRepository
    {
        public DatabaseAccessRepository(AuthDbContext context) : base(context)
        {
        }

        public IEnumerable<DatabaseAccess> GetByServerIpId(int serverIpId)
        {
            return _dbSet.AsNoTracking()
                .Where(da => da.ServerIpId == serverIpId && da.IsActive)
                .OrderBy(da => da.DatabaseName)
                .ToList();
        }

        public DatabaseAccess GetByServerIpAndDatabase(int serverIpId, string databaseName)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefault(da => da.ServerIpId == serverIpId
                    && da.DatabaseName == databaseName
                    && da.IsActive);
        }

        public bool DatabaseAccessExists(int serverIpId, string databaseName)
        {
            return _dbSet.Any(da => da.ServerIpId == serverIpId
                && da.DatabaseName == databaseName
                && da.IsActive);
        }

        public IEnumerable<DatabaseAccess> GetAccessibleDatabasesByServerId(int serverIpId)
        {
            return _dbSet.AsNoTracking()
                .Where(da => da.ServerIpId == serverIpId
                    && da.IsActive
                    && da.HasAccess)
                .OrderBy(da => da.DatabaseName)
                .ToList();
        }
    }
}
