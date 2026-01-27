using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    [AuthorizeUser]
    public class SalaryGarbgeController : BaseController
    {
        private readonly ISalaryGarbgeScanService _scanService;
        private readonly IUserToolService _userToolService;
        private readonly IAuthUnitOfWork _unitOfWork;

        public SalaryGarbgeController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
            _scanService = new SalaryGarbgeScanService(_unitOfWork);
            _userToolService = new UserToolService(_unitOfWork);
        }

        // Check if the user request for any admin action it will redirect to admin dashboard else if the user is admin and request for the user action it will return access denied view or redirect to the '/'.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsAdmin)
            {
                var actionName = filterContext.ActionDescriptor.ActionName;
                if (actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase) ||
                    actionName.Equals("Dashboard", System.StringComparison.OrdinalIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("~/AdminDashboard");
                    return;
                }

                ViewBag.Message = "Administrators cannot access the User Dashboard.";
                filterContext.Result = View("AccessDenied");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: SalaryGarbge/Index
        public ActionResult Index()
        {
            return View("~/Views/SalaryGarbge/Index.cshtml");
        }

        // GET: SalaryGarbge/GetActiveServers
        [HttpGet]
        public JsonResult GetActiveServers()
        {
            var result = _scanService.GetActiveServerIps();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: SalaryGarbge/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            var result = _scanService.GetDatabasesOnServer(serverIpId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: SalaryGarbge/GetAccessibleDatabases
        [HttpGet]
        public JsonResult GetAccessibleDatabases(int serverIpId)
        {
            try
            {
                // Get only databases with access granted from DatabaseAccess table
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

                return Json(ApiResponse<object>.Success(accessibleDatabases), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse<object>.Fail("Error loading accessible databases: " + ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        // POST: SalaryGarbge/ScanDatabase
        [HttpPost]
        public JsonResult ScanDatabase(int serverIpId, string databaseName)
        {
            var result = _scanService.ScanDatabase(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanAll
        [HttpPost]
        public JsonResult ScanAll()
        {
            var result = _scanService.ScanAllDatabases();

            if (!result.Success)
            {
                return Json(ApiResponse<GarbageScanResultDto>.Fail(result.Message));
            }

            return Json(ApiResponse<GarbageScanResultDto>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanProblematicDatabase
        [HttpPost]
        public JsonResult ScanProblematicDatabase(int serverIpId, string databaseName)
        {
            var result = _scanService.ScanDatabaseForProblematic(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: SalaryGarbge/ScanAllProblematic
        [HttpPost]
        public JsonResult ScanAllProblematic()
        {
            var result = _scanService.ScanAllProblematicDatabases();

            if (!result.Success)
            {
                return Json(ApiResponse<ProblematicScanResultDto>.Fail(result.Message));
            }

            return Json(ApiResponse<ProblematicScanResultDto>.Success(result.Data));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
