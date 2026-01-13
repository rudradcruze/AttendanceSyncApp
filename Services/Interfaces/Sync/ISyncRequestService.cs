using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Services.Interfaces.Sync
{
    public interface ISyncRequestService
    {
        ServiceResult<PagedResultDto<SyncRequestDto>> GetUserRequestsPaged(int userId, int page, int pageSize);
        ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, string userEmail, int sessionId);
        ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids);
        ServiceResult<IEnumerable<UserDto>> GetAllUsers();
        ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies();
        ServiceResult<IEnumerable<Tool>> GetActiveTools();
    }
}
