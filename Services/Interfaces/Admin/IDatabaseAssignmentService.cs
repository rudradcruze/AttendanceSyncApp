using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IDatabaseAssignmentService
    {
        ServiceResult AssignDatabase(AssignDatabaseDto dto, int adminUserId);
        ServiceResult<AssignDatabaseDto> GetAssignment(int requestId);
        ServiceResult UpdateAssignment(int requestId, AssignDatabaseDto dto, int adminUserId);
    }
}
