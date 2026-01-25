using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.BranchIssue;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;

namespace AttandanceSyncApp.Services.Interfaces.BranchIssue
{
    public interface IBranchIssueService
    {
        ServiceResult<IEnumerable<ServerIpDto>> GetAllServerIps();
        ServiceResult<IEnumerable<DatabaseListDto>> GetDatabasesForServer(int serverIpId);
        ServiceResult<IEnumerable<ProblemBranchDto>> GetProblemBranches(int serverIpId, string databaseName, string monthStartDate, string locationId);
        ServiceResult<string> GetLastMonthDate(int serverIpId, string databaseName);
        ServiceResult<string> ReprocessBranch(ReprocessBranchRequestDto request);
    }
}
