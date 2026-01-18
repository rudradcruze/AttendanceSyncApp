using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminToolsController : BaseController
    {
        private readonly IToolManagementService _toolService;

        public AdminToolsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _toolService = new ToolManagementService(unitOfWork);
        }

        // GET: AdminTools/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Tools.cshtml");
        }

        // GET: AdminTools/GetTools
        [HttpGet]
        public JsonResult GetTools(int page = 1, int pageSize = 20)
        {
            var result = _toolService.GetToolsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminTools/GetTool
        [HttpGet]
        public JsonResult GetTool(int id)
        {
            var result = _toolService.GetToolById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<ToolDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<ToolDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminTools/CreateTool
        [HttpPost]
        public JsonResult CreateTool(ToolCreateDto dto)
        {
            var result = _toolService.CreateTool(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/UpdateTool
        [HttpPost]
        public JsonResult UpdateTool(ToolUpdateDto dto)
        {
            var result = _toolService.UpdateTool(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/DeleteTool
        [HttpPost]
        public JsonResult DeleteTool(int id)
        {
            var result = _toolService.DeleteTool(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/ToggleToolStatus
        [HttpPost]
        public JsonResult ToggleToolStatus(int id)
        {
            var result = _toolService.ToggleToolStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
