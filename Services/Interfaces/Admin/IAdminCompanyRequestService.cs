using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminCompanyRequestService
    {
        ServiceResult<PagedResultDto<CompanyRequestListDto>> GetAllRequestsPaged(int page, int pageSize);
        ServiceResult<CompanyRequestListDto> GetRequestById(int id);
        ServiceResult UpdateRequestStatus(int requestId, string status);
        ServiceResult AcceptRequest(int requestId);
        ServiceResult RejectRequest(int requestId);
        ServiceResult AssignDatabase(int requestId, int adminUserId);
        ServiceResult<DatabaseConfigDto> GetDatabaseConfigForRequest(int requestId);
        ServiceResult<int> GetNewRequestsCount(int lastKnownId);
        ServiceResult<int> GetNewestRequestId();
    }
}
