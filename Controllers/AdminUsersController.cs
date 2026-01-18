using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminUsersController : BaseController
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminUserService = new AdminUserService(unitOfWork);
        }

        // GET: AdminUsers/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Users.cshtml");
        }

        // GET: AdminUsers/GetUsers
        [HttpGet]
        public JsonResult GetUsers(int page = 1, int pageSize = 20)
        {
            var result = _adminUserService.GetUsersPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUsers/GetUser
        [HttpGet]
        public JsonResult GetUser(int id)
        {
            var result = _adminUserService.GetUserById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<UserListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<UserListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminUsers/UpdateUser
        [HttpPost]
        public JsonResult UpdateUser(UserListDto userDto)
        {
            var result = _adminUserService.UpdateUser(userDto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUsers/ToggleUserStatus
        [HttpPost]
        public JsonResult ToggleUserStatus(int userId)
        {
            var result = _adminUserService.ToggleUserStatus(userId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
