using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    [AdminAuthorize]
    public class AdminDatabaseAccessController : BaseController
    {
        private readonly IDatabaseAccessService _dbAccessService;
        private readonly IServerIpManagementService _serverIpService;

        public AdminDatabaseAccessController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _dbAccessService = new DatabaseAccessService(unitOfWork);
            _serverIpService = new ServerIpManagementService(unitOfWork);
        }

        public ActionResult Index()
        {
            return View("~/Views/Admin/SalaryGarbge/DatabaseAccess.cshtml");
        }

        [HttpGet]
        public JsonResult GetServerIps()
        {
            var result = _serverIpService.GetServerIpsPaged(1, 1000);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var activeServers = result.Data.Data.Where(s => s.IsActive).ToList();
            return Json(ApiResponse<object>.Success(activeServers), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDatabasesWithAccess(int serverIpId)
        {
            var result = _dbAccessService.GetDatabasesWithAccessStatus(serverIpId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddDatabaseAccess(int serverIpId, string databaseName)
        {
            var result = _dbAccessService.AddDatabaseAccess(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        [HttpPost]
        public JsonResult UpdateDatabaseAccess(int serverIpId, string databaseName, bool hasAccess)
        {
            var result = _dbAccessService.UpdateDatabaseAccess(serverIpId, databaseName, hasAccess);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        [HttpPost]
        public JsonResult RemoveDatabaseAccess(int serverIpId, string databaseName)
        {
            var result = _dbAccessService.RemoveDatabaseAccess(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
