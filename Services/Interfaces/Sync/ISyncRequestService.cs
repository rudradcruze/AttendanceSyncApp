using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Services.Interfaces.Sync
{
    public interface ISyncRequestService
    {
        ServiceResult<PagedResultDto<SyncRequestDto>> GetUserRequestsPaged(int userId, int? companyId, int page, int pageSize, string sortColumn = "ToDate", string sortDirection = "DESC");
        ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, int sessionId);
        ServiceResult CancelSyncRequest(int requestId, int userId);
        ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids);
        ServiceResult<IEnumerable<StatusDto>> GetExternalStatusesByIds(int userId, int companyId, int[] ids);
        ServiceResult<IEnumerable<UserDto>> GetAllUsers();
        ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies();
        ServiceResult<IEnumerable<Tool>> GetActiveTools();
        ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees();
        ServiceResult<IEnumerable<UserCompanyDatabaseDto>> GetUserCompanyDatabases(int userId);
        ServiceResult<int> CreateOnTheFlySynchronization(SyncRequestCreateDto dto, int userId, int sessionId);
    }
}
