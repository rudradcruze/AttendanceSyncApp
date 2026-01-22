using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.ConcurrentSimulation;
using AttandanceSyncApp.Services.Interfaces.ConcurrentSimulation;

namespace AttandanceSyncApp.Controllers.ConcurrentSimulation
{
    [AuthorizeUser]
    public class ConcurrentSimulationController : BaseController
    {
        private readonly IConcurrentSimulationService _service;

        public ConcurrentSimulationController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _service = new ConcurrentSimulationService(unitOfWork);
        }

        // GET: ConcurrentSimulation/Index
        public ActionResult Index()
        {
            return View("~/Views/ConcurrentSimulation/Index.cshtml");
        }

        // GET: ConcurrentSimulation/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps()
        {
            var result = _service.GetAllServerIps();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: ConcurrentSimulation/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            var result = _service.GetDatabasesForServer(serverIpId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: ConcurrentSimulation/GetPeriodEndData
        [HttpGet]
        public JsonResult GetPeriodEndData(int serverIpId, string databaseName)
        {
            var result = _service.GetPeriodEndData(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: ConcurrentSimulation/HitConcurrent
        [HttpPost]
        public JsonResult HitConcurrent(HitConcurrentRequestDto request)
        {
            var result = _service.HitConcurrent(request);

            if (!result.Success)
            {
                return Json(ApiResponse<HitConcurrentResponseDto>.Fail(result.Message));
            }

            return Json(ApiResponse<HitConcurrentResponseDto>.Success(result.Data, result.Message));
        }
    }
}
