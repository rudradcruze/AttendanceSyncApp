using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.AttandanceSync;
using AttandanceSyncApp.Services.ConcurrentSimulation;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.Interfaces.ConcurrentSimulation;

namespace AttandanceSyncApp.Controllers.ConcurrentSimulation
{
    /// <summary>
    /// Handles concurrent processing simulation for testing
    /// salary period processing under multiple simultaneous requests.
    /// </summary>
    [AuthorizeUser]
    public class ConcurrentSimulationController : BaseController
    {
        /// Concurrent simulation service for testing operations.
        private readonly IConcurrentSimulationService _service;

        /// for checking tool list
        private readonly ISyncRequestService _syncRequestService;

        /// User tool service for managing user-assigned tools.
        private readonly IUserToolService _userToolService;

        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// Initializes controller with default services.
        public ConcurrentSimulationController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _unitOfWork = new AuthUnitOfWork();
            _userToolService = new UserToolService(_unitOfWork);
            _syncRequestService = new SyncRequestService(_unitOfWork);
            _service = new ConcurrentSimulationService(unitOfWork);
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
            var validToolNames = new[] { "Concurrent Simulation", "Concurrent Simulation Tool", "ConcurrentSimulation", "ConcurrentSimulationTool" };
            var tools = _syncRequestService.GetActiveTools();
            if (!tools.Success) return false;

            var targetTool = tools.Data.FirstOrDefault(t => validToolNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase));
            if (targetTool == null) return false;

            return _userToolService.UserHasToolAccess(userId, targetTool.Id);
        }

        // GET: ConcurrentSimulation/Index
        public ActionResult Index()
        {
            if (!HasAttendanceToolAccess(CurrentUserId))
            {
                ViewBag.Message = "You do not have access to the Attendance Sync tool. Please request access from your administrator.";
                return View("AccessDenied");
            }
            // Return the concurrent simulation testing view
            return View("~/Views/ConcurrentSimulation/Index.cshtml");
        }



        // GET: ConcurrentSimulation/GetServerIps
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

        // GET: ConcurrentSimulation/GetDatabases
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

        // GET: ConcurrentSimulation/GetPeriodEndData
        [HttpGet]
        public JsonResult GetPeriodEndData(int serverIpId, string databaseName)
        {
            // Retrieve period end data for concurrent processing simulation
            var result = _service.GetPeriodEndData(serverIpId, databaseName);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return period end data
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: ConcurrentSimulation/HitConcurrent
        [HttpPost]
        public JsonResult HitConcurrent(HitConcurrentRequestDto request)
        {
            try
            {
                // Execute concurrent processing simulation with the specified parameters
                var result = _service.HitConcurrent(request);

                // If simulation fails, return error response
                if (!result.Success)
                {
                    return Json(ApiResponse<HitConcurrentResponseDto>.Fail(result.Message));
                }

                // Return simulation results
                return Json(ApiResponse<HitConcurrentResponseDto>.Success(result.Data, result.Message));
            }
            catch (System.Exception ex)
            {
                // Catch and return any unexpected errors during simulation
                return Json(ApiResponse<HitConcurrentResponseDto>.Fail($"Error: {ex.Message}. Inner: {ex.InnerException?.Message}"));
            }
        }
    }
}
