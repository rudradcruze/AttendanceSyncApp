using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;

namespace AttandanceSyncApp.Services.Interfaces.SalaryGarbge
{
    public interface IDatabaseAccessService
    {
        ServiceResult<IEnumerable<DatabaseAccessListDto>> GetDatabasesWithAccessStatus(int serverIpId);
        ServiceResult AddDatabaseAccess(int serverIpId, string databaseName);
        ServiceResult UpdateDatabaseAccess(int serverIpId, string databaseName, bool hasAccess);
        ServiceResult RemoveDatabaseAccess(int serverIpId, string databaseName);
    }
}
