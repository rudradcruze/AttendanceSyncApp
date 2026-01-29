using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.AttandanceSync;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    /// <summary>
    /// User-facing controller for salary garbage scanning operations.
    /// Allows authorized users to scan databases for problematic salary records,
    /// identify data integrity issues, and view scan results.
    /// </summary>
    [AuthorizeUser]
    public class SalaryGarbgeController : BaseController
    {
        /// Service for performing salary garbage scan operations.
        private readonly ISalaryGarbgeScanService _scanService;

        /// for checking tool list
        private readonly ISyncRequestService _syncRequestService;

        /// Service for managing user tool access and permissions.
        private readonly IUserToolService _userToolService;

        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// Initializes controller with default services.
        public SalaryGarbgeController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
            _scanService = new SalaryGarbgeScanService(_unitOfWork);
            _syncRequestService = new SyncRequestService(_unitOfWork);
            _userToolService = new UserToolService(_unitOfWork);
        }

        /// <summary>
        /// Enforces access control to prevent admins from accessing user views.
        /// Redirects admins to the admin dashboard or shows access denied.
        /// </summary>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if the current user is an admin
            if (IsAdmin)
            {
                var actionName = filterContext.ActionDescriptor.ActionName;
                // Allow admins to access Index or Dashboard actions by redirecting to admin area
                if (actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase) ||
                    actionName.Equals("Dashboard", System.StringComparison.OrdinalIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("~/AdminDashboard");
                    return;
                }

                // Deny admin access to other user-specific actions
                ViewBag.Message = "Administrators cannot access the User Dashboard.";
                filterContext.Result = View("AccessDenied");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // tool access or not
        private bool HasAttendanceToolAccess(int userId)
        {
            var validToolNames = new[] { "Salary Garbage", "SalaryGarbage", "Salary Garbage Tool", "SalaryGarbageTool" };
            var tools = _syncRequestService.GetActiveTools();
            if (!tools.Success) return false;

            var targetTool = tools.Data.FirstOrDefault(t => validToolNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase));
            if (targetTool == null) return false;

            return _userToolService.UserHasToolAccess(userId, targetTool.Id);
        }

        // GET: SalaryGarbge/Index
        public ActionResult Index()
        {
            if (!HasAttendanceToolAccess(CurrentUserId))
            {
                ViewBag.Message = "You do not have access to the Attendance Sync tool. Please request access from your administrator.";
                return View("AccessDenied");
            }
            // Return the main salary garbage scanning view for users
            return View("~/Views/SalaryGarbge/Index.cshtml");
        }

        // GET: SalaryGarbge/GetActiveServers
        [HttpGet]
        public JsonResult GetActiveServers()
        {
            // Retrieve all active server IPs available for scanning
            var result = _scanService.GetActiveServerIps();

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return list of active servers
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: SalaryGarbge/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            // Retrieve all databases available on the specified server
            var result = _scanService.GetDatabasesOnServer(serverIpId);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return list of databases on the server
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: SalaryGarbge/GetAccessibleDatabases
        [HttpGet]
        public JsonResult GetAccessibleDatabases(int serverIpId)
        {
            try
            {
                // Retrieve only databases that have been granted scan access
                // from the DatabaseAccess configuration table
                var accessibleDatabases = _unitOfWork.DatabaseAccess
                    .GetAccessibleDatabasesByServerId(serverIpId)
                    .Select(da => new
                    {
                        DatabaseName = da.DatabaseName,
                        HasAccess = da.HasAccess,
                        ExistsInAccessTable = true
                    })
                    .OrderBy(d => d.DatabaseName)
                    .ToList();

                // Return filtered list of accessible databases
                return Json(ApiResponse<object>.Success(accessibleDatabases), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // If an error occurs during retrieval, return error response
                return Json(ApiResponse<object>.Fail("Error loading accessible databases: " + ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        // POST: SalaryGarbge/ScanDatabase
        [HttpPost]
        public JsonResult ScanDatabase(int serverIpId, string databaseName)
        {
            // Perform a salary garbage scan on a specific database
            // to identify problematic salary records
            var result = _scanService.ScanDatabase(serverIpId, databaseName);

            // If scan fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return scan results with identified garbage records
            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanAll
        [HttpPost]
        public JsonResult ScanAll()
        {
            // Perform a comprehensive scan across all accessible databases
            // on all active servers to find salary garbage records
            var result = _scanService.ScanAllDatabases();

            // If scan fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<GarbageScanResultDto>.Fail(result.Message));
            }

            // Return aggregated scan results from all databases
            return Json(ApiResponse<GarbageScanResultDto>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanProblematicDatabase
        [HttpPost]
        public JsonResult ScanProblematicDatabase(int serverIpId, string databaseName)
        {
            // Perform a focused scan on a specific database
            // to identify particularly problematic salary records
            var result = _scanService.ScanDatabaseForProblematic(serverIpId, databaseName);

            // If scan fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return problematic salary records found
            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanAllProblematic
        [HttpPost]
        public JsonResult ScanAllProblematic()
        {
            // Perform a comprehensive scan across all accessible databases
            // to identify the most problematic salary records requiring immediate attention
            var result = _scanService.ScanAllProblematicDatabases();

            // If scan fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<ProblematicScanResultDto>.Fail(result.Message));
            }

            // Return aggregated problematic scan results
            return Json(ApiResponse<ProblematicScanResultDto>.Success(result.Data));
        }

        /// <summary>
        /// Disposes resources used by the controller, including the unit of work.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose the unit of work to release database connections
                _unitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
