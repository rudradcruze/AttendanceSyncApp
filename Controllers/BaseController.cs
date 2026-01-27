using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Provides common authentication and user-related functionality for all application controllers.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Authentication service used to resolve the current user and session.
        /// </summary>
        protected readonly IAuthService _authService;

        private UserDto _currentUser;

        /// <summary>
        /// Initializes the controller with default authentication dependencies.
        /// </summary>
        protected BaseController()
        {
            var unitOfWork = new AuthUnitOfWork();
            var googleAuth = new GoogleAuthService();
            var sessionService = new SessionService(unitOfWork);
            _authService = new AuthService(unitOfWork, googleAuth, sessionService);
        }

        /// <summary>
        /// Initializes the controller with a provided authentication service.
        /// </summary>
        /// <param name="authService">The authentication service instance.</param>
        protected BaseController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Gets the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// The user is resolved lazily using the session token and cached
        /// for the lifetime of the request.
        /// </remarks>
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

        /// <summary>
        /// Indicates whether the current request is authenticated.
        /// </summary>
        protected bool IsAuthenticated => CurrentUser != null;

        /// <summary>
        /// Indicates whether the current user has administrator privileges.
        /// </summary>
        protected bool IsAdmin => CurrentUser?.Role == "ADMIN";

        /// <summary>
        /// Gets the current user's unique identifier.
        /// </summary>
        /// <remarks>
        /// Returns <c>0</c> when no authenticated user exists.
        /// </remarks>
        protected int CurrentUserId => CurrentUser?.Id ?? 0;

        /// <summary>
        /// Gets the current user's active login session ID.
        /// </summary>
        /// <remarks>
        /// This property retrieves the session token from the current context and
        /// looks up the corresponding login session in the authentication store.
        /// If no token exists or the token is invalid, the method returns <c>0</c>.
        /// </remarks>
        /// <value>
        /// The unique identifier of the current login session; otherwise <c>0</c>
        /// if the user is not logged in or the session cannot be resolved.
        /// </value>
        protected int CurrentSessionId
        {
            get
            {
                // Retrieve the current session token; return 0 if missing
                var token = GetSessionToken();
                if (string.IsNullOrEmpty(token))
                    return 0;

                // Resolve the session using the authentication unit of work
                using (var unitOfWork = new AuthUnitOfWork())
                {
                    var session = unitOfWork.LoginSessions.GetByToken(token);
                    return session?.Id ?? 0;
                }
            }
        }

        // get the session token for authorization
        protected string GetSessionToken()
        {
            // Check cookie first
            var cookie = Request.Cookies["SessionToken"];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                return cookie.Value;
            }

            // Check Authorization header if all okay then return the token else return null
            var authHeader = Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring(7);
            }

            return null;
        }

        // set the session cookie so that next time user can sign in automatic for next '5' days it will only for the login. 
        protected void SetSessionCookie(string token)
        {
            var cookie = new HttpCookie("SessionToken", token)
            {
                HttpOnly = true, // this allow cookie is only for http
                Secure = Request.IsSecureConnection, // encrypt this cookie
                Expires = System.DateTime.Now.AddDays(5), // life of the cookie 
                Path = "/" // path of the cookie
            };
            Response.Cookies.Add(cookie); // return / add the cookie
        }

        // clear the session cookie this is use for clear the auto login if the http cookie available (if clear -> user unable to auto login next time) this will only for the login

        protected void ClearSessionCookie()
        {
            var cookie = new HttpCookie("SessionToken", "")
            {
                Expires = System.DateTime.Now.AddDays(-1),
                Path = "/"
            };
            Response.Cookies.Add(cookie);
        }

        // get the session info of device, brower and ip address
        protected SessionDto GetSessionInfo()
        {
            return new SessionDto
            {
                Device = Request.UserAgent,
                Browser = Request.Browser?.Browser,
                IPAddress = GetClientIP()
            };
        }

        // get the client ip address
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
