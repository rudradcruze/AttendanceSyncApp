using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    [AuthorizeUser]
    public class SalaryGarbgeController : BaseController
    {
        private readonly ISalaryGarbgeScanService _scanService;

        public SalaryGarbgeController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _scanService = new SalaryGarbgeScanService(unitOfWork);
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
    }
}
