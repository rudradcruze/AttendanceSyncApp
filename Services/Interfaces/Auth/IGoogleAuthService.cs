using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;

namespace AttandanceSyncApp.Services.Interfaces.Auth
{
    public interface IGoogleAuthService
    {
        string GetAuthorizationUrl(string state);
        ServiceResult<GoogleUserInfo> ExchangeCodeForTokens(string code);
        ServiceResult<GoogleUserInfo> ValidateIdToken(string idToken);
    }
}
