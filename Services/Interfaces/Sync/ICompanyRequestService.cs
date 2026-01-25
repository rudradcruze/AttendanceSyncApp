using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Services.Interfaces.Sync
{
    public interface ICompanyRequestService
    {
        ServiceResult<PagedResultDto<CompanyRequestDto>> GetUserRequestsPaged(int userId, int page, int pageSize);
        ServiceResult<int> CreateRequest(CompanyRequestCreateDto dto, int userId, int sessionId);
        ServiceResult CancelRequest(int requestId, int userId);
        ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees();
        ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies();
        ServiceResult<IEnumerable<Tool>> GetActiveTools();
    }
}
