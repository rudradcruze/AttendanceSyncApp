using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminCompanyRequestService
    {
        ServiceResult<PagedResultDto<CompanyRequestListDto>> GetAllRequestsPaged(int page, int pageSize);
        ServiceResult<CompanyRequestListDto> GetRequestById(int id);
        ServiceResult UpdateRequestStatus(int requestId, string status);
    }
}
