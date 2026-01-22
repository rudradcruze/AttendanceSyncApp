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

        // GET: SalaryGarbge/Index
        public ActionResult Index()
        {
            if (!HasSalaryGarbageToolAccess(CurrentUserId))
            {
                ViewBag.Message = "You do not have access to the Salary Garbage tool. Please request access from your administrator.";
                return View("AccessDenied");
            }
            return View("~/Views/SalaryGarbge/Index.cshtml");
        }

        private bool HasSalaryGarbageToolAccess(int userId)
        {
            var validToolNames = new[] { "Salary Garbage", "SalaryGarbge", "Salary Garbge", "Salary Issue" };

            // Get all active tools assigned to the user
            var userTools = _unitOfWork.UserTools.GetActiveToolsByUserId(userId);

            // Check if any of the assigned tools match the valid names
            return userTools.Any(ut => validToolNames.Contains(ut.Tool.Name, StringComparer.OrdinalIgnoreCase));
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
