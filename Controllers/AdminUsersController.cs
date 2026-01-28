using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Handles user management operations for administrators,
    /// including viewing, updating, and managing user status.
    /// </summary>
    [AdminAuthorize]
    public class AdminUsersController : BaseController
    {
        /// User management service for business logic.
        private readonly IAdminUserService _adminUserService;

        /// Initializes controller with default services.
        public AdminUsersController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminUserService = new AdminUserService(unitOfWork);
        }

        // GET: AdminUsers/Index
        public ActionResult Index()
        {
            // Return the user management view
            return View("~/Views/Admin/Users.cshtml");
        }

        // GET: AdminUsers/GetUsers
        [HttpGet]
        public JsonResult GetUsers(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of users
            var result = _adminUserService.GetUsersPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return user data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUsers/GetUser
        [HttpGet]
        public JsonResult GetUser(int id)
        {
            // Retrieve specific user details by ID
            var result = _adminUserService.GetUserById(id);

            // If user not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<UserListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return user details
            return Json(ApiResponse<UserListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminUsers/UpdateUser
        [HttpPost]
        public JsonResult UpdateUser(UserListDto userDto)
        {
            // Attempt to update user information
            var result = _adminUserService.UpdateUser(userDto);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUsers/ToggleUserStatus
        [HttpPost]
        public JsonResult ToggleUserStatus(int userId)
        {
            // Toggle user active/inactive status
            var result = _adminUserService.ToggleUserStatus(userId);

            // If toggle fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
