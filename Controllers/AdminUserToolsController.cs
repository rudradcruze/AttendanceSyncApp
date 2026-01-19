using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminUserToolsController : BaseController
    {
        private readonly IUserToolService _userToolService;
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminUserToolsController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
            _userToolService = new UserToolService(_unitOfWork);
        }

        // GET: AdminUserTools/Index
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "UserTools";
            return View("~/Views/Admin/UserTools.cshtml");
        }

        // GET: AdminUserTools/GetAssignments
        [HttpGet]
        public JsonResult GetAssignments(int page = 1, int pageSize = 20)
        {
            var result = _userToolService.GetAllAssignmentsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminUserTools/AssignTool
        [HttpPost]
        public JsonResult AssignTool(UserToolAssignDto dto)
        {
            var result = _userToolService.AssignToolToUser(dto, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUserTools/RevokeTool
        [HttpPost]
        public JsonResult RevokeTool(UserToolRevokeDto dto)
        {
            var result = _userToolService.RevokeToolFromUser(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUserTools/UnrevokeTool
        [HttpPost]
        public JsonResult UnrevokeTool(int userId, int toolId)
        {
            var result = _userToolService.UnrevokeToolAssignment(userId, toolId, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminUserTools/GetUserAssignments
        [HttpGet]
        public JsonResult GetUserAssignments(int userId)
        {
            var result = _userToolService.GetToolAssignmentsByUserId(userId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUserTools/GetAllUsers - Get users for dropdown
        [HttpGet]
        public JsonResult GetAllUsers()
        {
            var users = _unitOfWork.Users.GetAll();
            var data = new System.Collections.Generic.List<object>();

            foreach (var u in users)
            {
                if (u.IsActive && u.Role != "ADMIN")
                {
                    data.Add(new { u.Id, u.Name, u.Email });
                }
            }

            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUserTools/GetAllTools - Get tools for dropdown
        [HttpGet]
        public JsonResult GetAllTools()
        {
            var tools = _unitOfWork.Tools.GetAll();
            var data = new System.Collections.Generic.List<object>();

            foreach (var t in tools)
            {
                if (t.IsActive)
                {
                    data.Add(new { t.Id, t.Name });
                }
            }

            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_unitOfWork as System.IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
