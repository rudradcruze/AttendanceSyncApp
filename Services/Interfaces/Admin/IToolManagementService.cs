using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IToolManagementService
    {
        ServiceResult<PagedResultDto<ToolDto>> GetToolsPaged(int page, int pageSize);
        ServiceResult<ToolDto> GetToolById(int id);
        ServiceResult CreateTool(ToolCreateDto dto);
        ServiceResult UpdateTool(ToolUpdateDto dto);
        ServiceResult DeleteTool(int id);
        ServiceResult ToggleToolStatus(int id);
    }
}
