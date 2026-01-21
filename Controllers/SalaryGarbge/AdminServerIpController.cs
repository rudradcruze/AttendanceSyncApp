using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    [AdminAuthorize]
    public class AdminServerIpController : BaseController
    {
        private readonly IServerIpManagementService _serverIpService;

        public AdminServerIpController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _serverIpService = new ServerIpManagementService(unitOfWork);
        }

        // GET: AdminServerIp/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/SalaryGarbge/ServerIp.cshtml");
        }

        // GET: AdminServerIp/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps(int page = 1, int pageSize = 20)
        {
            var result = _serverIpService.GetServerIpsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminServerIp/GetServerIp
        [HttpGet]
        public JsonResult GetServerIp(int id)
        {
            var result = _serverIpService.GetServerIpById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<ServerIpDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<ServerIpDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminServerIp/CreateServerIp
        [HttpPost]
        public JsonResult CreateServerIp(ServerIpCreateDto dto)
        {
            var result = _serverIpService.CreateServerIp(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/UpdateServerIp
        [HttpPost]
        public JsonResult UpdateServerIp(ServerIpUpdateDto dto)
        {
            var result = _serverIpService.UpdateServerIp(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/DeleteServerIp
        [HttpPost]
        public JsonResult DeleteServerIp(int id)
        {
            var result = _serverIpService.DeleteServerIp(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/ToggleServerIpStatus
        [HttpPost]
        public JsonResult ToggleServerIpStatus(int id)
        {
            var result = _serverIpService.ToggleServerIpStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
