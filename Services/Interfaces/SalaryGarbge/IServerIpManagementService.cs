using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;

namespace AttandanceSyncApp.Services.Interfaces.SalaryGarbge
{
    public interface IServerIpManagementService
    {
        ServiceResult<PagedResultDto<ServerIpDto>> GetServerIpsPaged(int page, int pageSize);
        ServiceResult<ServerIpDto> GetServerIpById(int id);
        ServiceResult CreateServerIp(ServerIpCreateDto dto);
        ServiceResult UpdateServerIp(ServerIpUpdateDto dto);
        ServiceResult DeleteServerIp(int id);
        ServiceResult ToggleServerIpStatus(int id);
    }
}
