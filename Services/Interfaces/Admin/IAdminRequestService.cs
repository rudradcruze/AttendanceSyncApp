using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminRequestService
    {
        ServiceResult<PagedResultDto<RequestListDto>> GetAllRequestsPaged(int page, int pageSize);
        ServiceResult<RequestListDto> GetRequestById(int id);
        ServiceResult UpdateRequestStatus(int requestId, string status);
    }
}
