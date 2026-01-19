using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminDatabaseAssignmentService
    {
        ServiceResult<PagedResultDto<DatabaseAssignListDto>> GetAllAssignmentsPaged(int page, int pageSize);
        ServiceResult<DatabaseAssignListDto> GetAssignmentById(int id);
        ServiceResult<DatabaseAssignListDto> GetAssignmentByRequestId(int companyRequestId);
        ServiceResult RevokeAssignment(int id);
        ServiceResult UnrevokeAssignment(int id);
    }
}
