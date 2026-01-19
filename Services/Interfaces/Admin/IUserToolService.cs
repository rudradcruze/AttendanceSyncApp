using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IUserToolService
    {
        ServiceResult<PagedResultDto<UserToolDto>> GetAllAssignmentsPaged(int page, int pageSize);
        ServiceResult<IEnumerable<AssignedToolDto>> GetUserAssignedTools(int userId);
        ServiceResult AssignToolToUser(UserToolAssignDto dto, int assignedBy);
        ServiceResult RevokeToolFromUser(UserToolRevokeDto dto);
        ServiceResult UnrevokeToolAssignment(int userId, int toolId, int assignedBy);
        ServiceResult<IEnumerable<UserToolDto>> GetToolAssignmentsByUserId(int userId);
        bool UserHasToolAccess(int userId, int toolId);
    }
}
