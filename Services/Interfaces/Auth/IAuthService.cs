using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;

namespace AttandanceSyncApp.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        ServiceResult<UserDto> LoginWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo);
        ServiceResult<UserDto> LoginAdmin(string email, string password, SessionDto sessionInfo);
        ServiceResult<UserDto> LoginUser(string email, string password, SessionDto sessionInfo);
        ServiceResult<UserDto> RegisterWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo);
        ServiceResult<UserDto> RegisterUser(RegisterDto registerDto, SessionDto sessionInfo);
        ServiceResult Logout(string sessionToken);
        ServiceResult<UserDto> GetCurrentUser(string sessionToken);
        bool ValidateSession(string sessionToken);
    }
}
