using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;

namespace AttandanceSyncApp.Services.Interfaces.Auth
{
    public interface ISessionService
    {
        ServiceResult<LoginSession> CreateSession(int userId, SessionDto sessionInfo);
        ServiceResult<LoginSession> GetActiveSession(string sessionToken);
        ServiceResult EndSession(string sessionToken);
        string GenerateSessionToken();
    }
}
