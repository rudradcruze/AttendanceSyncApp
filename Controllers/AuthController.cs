using System.Configuration;
using System.Web.Mvc;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Auth;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IGoogleAuthService _googleAuthService;

        public AuthController() : base()
        {
            _googleAuthService = new GoogleAuthService();
        }

        public AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
            : base(authService)
        {
            _googleAuthService = googleAuthService;
        }

        // GET: Auth/Login
        public ActionResult Login()
        {
            if (IsAuthenticated)
            {
                if (IsAdmin)
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                return RedirectToAction("Index", "Attandance");
            }

            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Attandance");
            }

            ViewBag.GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            return View();
        }

        // POST: Auth/GoogleSignIn
        [HttpPost]
        public JsonResult GoogleSignIn(GoogleAuthDto googleAuth)
        {
            var sessionInfo = GetSessionInfo();
            var result = _authService.LoginWithGoogle(googleAuth, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/GoogleSignUp
        [HttpPost]
        public JsonResult GoogleSignUp(GoogleAuthDto googleAuth)
        {
            var sessionInfo = GetSessionInfo();
            var result = _authService.RegisterWithGoogle(googleAuth, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/AdminLogin
        [HttpPost]
        public JsonResult AdminLogin(string email, string password)
        {
            var sessionInfo = GetSessionInfo();
            var result = _authService.LoginAdmin(email, password, sessionInfo);

            if (result.Success)
            {
                SetSessionCookie(result.Data.SessionToken);
                return Json(ApiResponse<UserDto>.Success(result.Data, result.Message));
            }

            return Json(ApiResponse<UserDto>.Fail(result.Message));
        }

        // POST: Auth/Logout
        [HttpPost]
        public JsonResult Logout()
        {
            var token = GetSessionToken();
            if (!string.IsNullOrEmpty(token))
            {
                _authService.Logout(token);
            }

            ClearSessionCookie();
            return Json(ApiResponse.Success("Logged out successfully"));
        }

        // GET: Auth/CurrentUser
        [HttpGet]
        public JsonResult CurrentUser()
        {
            var token = GetSessionToken();
            if (string.IsNullOrEmpty(token))
            {
                return Json(ApiResponse<UserDto>.Fail("Not authenticated"), JsonRequestBehavior.AllowGet);
            }

            var result = _authService.GetCurrentUser(token);

            if (!result.Success)
            {
                return Json(ApiResponse<UserDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<UserDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Auth/GoogleCallback
        public ActionResult GoogleCallback(string code, string state, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
                return View("Login");
            }

            // This is handled client-side with the Google Sign-In button
            return RedirectToAction("Login");
        }
    }
}
