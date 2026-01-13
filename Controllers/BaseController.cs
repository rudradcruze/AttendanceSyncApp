using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IAuthService _authService;
        private UserDto _currentUser;

        protected BaseController()
        {
            var unitOfWork = new AuthUnitOfWork();
            var googleAuth = new GoogleAuthService();
            var sessionService = new SessionService(unitOfWork);
            _authService = new AuthService(unitOfWork, googleAuth, sessionService);
        }

        protected BaseController(IAuthService authService)
        {
            _authService = authService;
        }

        protected UserDto CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    var token = GetSessionToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        var result = _authService.GetCurrentUser(token);
                        if (result.Success)
                        {
                            _currentUser = result.Data;
                        }
                    }
                }
                return _currentUser;
            }
        }

        protected bool IsAuthenticated => CurrentUser != null;
        protected bool IsAdmin => CurrentUser?.Role == "ADMIN";
        protected int CurrentUserId => CurrentUser?.Id ?? 0;
        protected int CurrentSessionId
        {
            get
            {
                var token = GetSessionToken();
                if (string.IsNullOrEmpty(token))
                    return 0;

                using (var unitOfWork = new AuthUnitOfWork())
                {
                    var session = unitOfWork.LoginSessions.GetByToken(token);
                    return session?.Id ?? 0;
                }
            }
        }

        protected string GetSessionToken()
        {
            // Check cookie first
            var cookie = Request.Cookies["SessionToken"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                return cookie.Value;
            }

            // Check Authorization header
            var authHeader = Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring(7);
            }

            return null;
        }

        protected void SetSessionCookie(string token)
        {
            var cookie = new HttpCookie("SessionToken", token)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                Expires = System.DateTime.Now.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Add(cookie);
        }

        protected void ClearSessionCookie()
        {
            var cookie = new HttpCookie("SessionToken", "")
            {
                Expires = System.DateTime.Now.AddDays(-1),
                Path = "/"
            };
            Response.Cookies.Add(cookie);
        }

        protected SessionDto GetSessionInfo()
        {
            return new SessionDto
            {
                Device = Request.UserAgent,
                Browser = Request.Browser?.Browser,
                IPAddress = GetClientIP()
            };
        }

        private string GetClientIP()
        {
            var ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
    }
}
