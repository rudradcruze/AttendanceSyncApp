using System.Collections.Generic;
using AttandanceSyncApp.Models.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge
{
    public interface IDatabaseAccessRepository : IRepository<DatabaseAccess>
    {
        IEnumerable<DatabaseAccess> GetByServerIpId(int serverIpId);
        DatabaseAccess GetByServerIpAndDatabase(int serverIpId, string databaseName);
        bool DatabaseAccessExists(int serverIpId, string databaseName);
        IEnumerable<DatabaseAccess> GetAccessibleDatabasesByServerId(int serverIpId);
    }
}
