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
    /// <summary>
    /// Manages tool assignments to users for administrators,
    /// including assigning, revoking, and restoring user tool access.
    /// </summary>
    [AdminAuthorize]
    public class AdminUserToolsController : BaseController
    {
        /// <summary>
        /// User tool service for managing tool assignments.
        /// </summary>
        private readonly IUserToolService _userToolService;

        /// <summary>
        /// Unit of work for accessing user and tool repositories.
        /// </summary>
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminUserToolsController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
            _userToolService = new UserToolService(_unitOfWork);
        }

        // GET: AdminUserTools/Index
        public ActionResult Index()
        {
            // Set active menu for UI navigation
            ViewBag.ActiveMenu = "UserTools";
            // Return the user tools management view
            return View("~/Views/Admin/UserTools.cshtml");
        }

        // GET: AdminUserTools/GetAssignments
        [HttpGet]
        public JsonResult GetAssignments(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of user tool assignments
            var result = _userToolService.GetAllAssignmentsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return assignment data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminUserTools/AssignTool
        [HttpPost]
        public JsonResult AssignTool(UserToolAssignDto dto)
        {
            // Assign a tool to a user with current admin as assigner
            var result = _userToolService.AssignToolToUser(dto, CurrentUserId);

            // If assignment fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUserTools/RevokeTool
        [HttpPost]
        public JsonResult RevokeTool(UserToolRevokeDto dto)
        {
            // Revoke a tool assignment from a user
            var result = _userToolService.RevokeToolFromUser(dto);

            // If revocation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminUserTools/UnrevokeTool
        [HttpPost]
        public JsonResult UnrevokeTool(int userId, int toolId)
        {
            // Restore a previously revoked tool assignment
            var result = _userToolService.UnrevokeToolAssignment(userId, toolId, CurrentUserId);

            // If restoration fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminUserTools/GetUserAssignments
        [HttpGet]
        public JsonResult GetUserAssignments(int userId)
        {
            // Retrieve all tool assignments for a specific user
            var result = _userToolService.GetToolAssignmentsByUserId(userId);

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return user's tool assignments
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUserTools/GetAllUsers - Get users for dropdown
        [HttpGet]
        public JsonResult GetAllUsers()
        {
            // Retrieve all users from repository
            var users = _unitOfWork.Users.GetAll();
            var data = new System.Collections.Generic.List<object>();

            // Filter active non-admin users for dropdown selection
            foreach (var u in users)
            {
                if (u.IsActive && u.Role != "ADMIN")
                {
                    data.Add(new { u.Id, u.Name, u.Email });
                }
            }

            // Return filtered user list
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminUserTools/GetAllTools - Get tools for dropdown
        [HttpGet]
        public JsonResult GetAllTools()
        {
            // Retrieve all tools from repository
            var tools = _unitOfWork.Tools.GetAll();
            var data = new System.Collections.Generic.List<object>();

            // Filter active tools for dropdown selection
            foreach (var t in tools)
            {
                if (t.IsActive)
                {
                    data.Add(new { t.Id, t.Name });
                }
            }

            // Return filtered tool list
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Disposes unit of work resources when controller is disposed.
        /// </summary>
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
