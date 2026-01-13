using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;

namespace AttandanceSyncApp.Services.Interfaces.Admin
{
    public interface IAdminUserService
    {
        ServiceResult<PagedResultDto<UserListDto>> GetUsersPaged(int page, int pageSize);
        ServiceResult<UserListDto> GetUserById(int id);
        ServiceResult UpdateUser(UserListDto userDto);
        ServiceResult ToggleUserStatus(int userId);
    }
}
