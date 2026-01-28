using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Manages tool catalog for administrators,
    /// including CRUD operations and tool status management.
    /// </summary>
    [AdminAuthorize]
    public class AdminToolsController : BaseController
    {
        /// <summary>
        /// Tool management service for business logic.
        /// </summary>
        private readonly IToolManagementService _toolService;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminToolsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _toolService = new ToolManagementService(unitOfWork);
        }

        // GET: AdminTools/Index
        public ActionResult Index()
        {
            // Return the tools management view
            return View("~/Views/Admin/Tools.cshtml");
        }

        // GET: AdminTools/GetTools
        [HttpGet]
        public JsonResult GetTools(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of tools
            var result = _toolService.GetToolsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return tool data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminTools/GetTool
        [HttpGet]
        public JsonResult GetTool(int id)
        {
            // Retrieve specific tool details by ID
            var result = _toolService.GetToolById(id);

            // If tool not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<ToolDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return tool details
            return Json(ApiResponse<ToolDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminTools/CreateTool
        [HttpPost]
        public JsonResult CreateTool(ToolCreateDto dto)
        {
            // Attempt to create a new tool
            var result = _toolService.CreateTool(dto);

            // If creation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/UpdateTool
        [HttpPost]
        public JsonResult UpdateTool(ToolUpdateDto dto)
        {
            // Attempt to update existing tool information
            var result = _toolService.UpdateTool(dto);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/DeleteTool
        [HttpPost]
        public JsonResult DeleteTool(int id)
        {
            // Attempt to delete the specified tool
            var result = _toolService.DeleteTool(id);

            // If deletion fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminTools/ToggleToolStatus
        [HttpPost]
        public JsonResult ToggleToolStatus(int id)
        {
            // Toggle tool active/inactive status
            var result = _toolService.ToggleToolStatus(id);

            // If toggle fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
