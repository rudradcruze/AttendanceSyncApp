using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.BranchIssue;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.BranchIssue;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.AttandanceSync;
using AttandanceSyncApp.Services.BranchIssue;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.Interfaces.BranchIssue;

namespace AttandanceSyncApp.Controllers.BranchIssue
{
    /// <summary>
    /// Handles branch-specific attendance sync issues,
    /// allowing users to identify and reprocess problematic branches.
    /// </summary>
    [AuthorizeUser]
    public class BranchIssueController : BaseController
    {
        /// Branch issue service for identifying and reprocessing problems.
        private readonly IBranchIssueService _service;

        // for tool list
        private readonly ISyncRequestService _syncRequestService;

        /// User tool service for managing user-assigned tools.
        private readonly IUserToolService _userToolService;

        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// Initializes controller with default services.
        public BranchIssueController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            var repository = new BranchIssueRepository();
            _unitOfWork = new AuthUnitOfWork();
            _service = new BranchIssueService(unitOfWork, repository);
            _syncRequestService = new SyncRequestService(_unitOfWork);
            _userToolService = new UserToolService(_unitOfWork);
        }

        /// <summary>
        /// Ensures administrators are redirected to admin dashboard
        /// or shown access denied for user-specific actions.
        /// </summary>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the current user is an administrator
            if (IsAdmin)
            {
                var actionName = filterContext.ActionDescriptor.ActionName;

                // Redirect admins from user dashboard to admin dashboard
                if (actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase) ||
                    actionName.Equals("Dashboard", System.StringComparison.OrdinalIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("~/AdminDashboard");
                    return;
                }

                // Block admins from accessing user-only features
                ViewBag.Message = "Administrators cannot access the User Dashboard.";
                filterContext.Result = View("AccessDenied");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        private bool HasAttendanceToolAccess(int userId)
        {
            var validToolNames = new[] { "Branch Issue", "Branch Issue Tool", "BranchIssue", "BranchIssueTool" };
            var tools = _syncRequestService.GetActiveTools();
            if (!tools.Success) return false;

            var targetTool = tools.Data.FirstOrDefault(t => validToolNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase));
            if (targetTool == null) return false;

            return _userToolService.UserHasToolAccess(userId, targetTool.Id);
        }

        // GET: BranchIssue/Index
        public ActionResult Index()
        {
            if (!HasAttendanceToolAccess(CurrentUserId))
            {
                ViewBag.Message = "You do not have access to the Attendance Sync tool. Please request access from your administrator.";
                return View("AccessDenied");
            }
            // Return the branch issue troubleshooting view
            return View("~/Views/BranchIssue/Index.cshtml");
        }

        // GET: BranchIssue/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps()
        {
            // Retrieve list of available server IPs for selection
            var result = _service.GetAllServerIps();

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return server IP list
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: BranchIssue/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            // Retrieve databases available on the selected server
            var result = _service.GetDatabasesForServer(serverIpId);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return database list for the server
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: BranchIssue/GetLastMonth
        [HttpGet]
        public JsonResult GetLastMonth(int serverIpId, string databaseName)
        {
            // Retrieve the last processed month date for the database
            var result = _service.GetLastMonthDate(serverIpId, databaseName);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return the last month date
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: BranchIssue/LoadProblemBranches
        [HttpPost]
        public JsonResult LoadProblemBranches(BranchIssueRequestDto request)
        {
            // Identify branches with attendance sync issues for the specified month
            var result = _service.GetProblemBranches(request.ServerIpId, request.DatabaseName, request.MonthStartDate, request.LocationId);

            // If identification fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return list of problematic branches
            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: BranchIssue/ReprocessBranch
        [HttpPost]
        public JsonResult ReprocessBranch(ReprocessBranchRequestDto request)
        {
            // Attempt to reprocess attendance data for the specified branch
            var result = _service.ReprocessBranch(request);

            // If reprocessing fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return reprocessing result
            return Json(ApiResponse<object>.Success(result.Data, result.Message));
        }
    }
}
