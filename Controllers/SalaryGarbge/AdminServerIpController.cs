using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    /// <summary>
    /// Manages database server IP configurations for salary garbage scanning.
    /// Allows administrators to add, update, delete, and toggle the status of
    /// database servers that contain salary data to be scanned for issues.
    /// </summary>
    [AdminAuthorize]
    public class AdminServerIpController : BaseController
    {
        /// Service for managing server IP configurations and operations.
        private readonly IServerIpManagementService _serverIpService;

        /// Initializes controller with default services.
        public AdminServerIpController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _serverIpService = new ServerIpManagementService(unitOfWork);
        }

        // GET: AdminServerIp/Index
        public ActionResult Index()
        {
            // Return the server IP management view
            return View("~/Views/Admin/SalaryGarbge/ServerIp.cshtml");
        }

        // GET: AdminServerIp/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps(int page = 1, int pageSize = 20)
        {
            // Retrieve server IPs with pagination support
            // for displaying in a grid or list view
            var result = _serverIpService.GetServerIpsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return paginated server IP data
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminServerIp/GetServerIp
        [HttpGet]
        public JsonResult GetServerIp(int id)
        {
            // Retrieve a single server IP configuration by ID
            // for viewing or editing purposes
            var result = _serverIpService.GetServerIpById(id);

            // If server IP not found or retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<ServerIpDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return server IP details
            return Json(ApiResponse<ServerIpDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminServerIp/CreateServerIp
        [HttpPost]
        public JsonResult CreateServerIp(ServerIpCreateDto dto)
        {
            // Create a new server IP configuration
            // for adding a database server to the scanning pool
            var result = _serverIpService.CreateServerIp(dto);

            // If creation fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/UpdateServerIp
        [HttpPost]
        public JsonResult UpdateServerIp(ServerIpUpdateDto dto)
        {
            // Update an existing server IP configuration
            // to modify connection details or other settings
            var result = _serverIpService.UpdateServerIp(dto);

            // If update fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/DeleteServerIp
        [HttpPost]
        public JsonResult DeleteServerIp(int id)
        {
            // Delete a server IP configuration
            // to remove it from the available scanning servers
            var result = _serverIpService.DeleteServerIp(id);

            // If deletion fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminServerIp/ToggleServerIpStatus
        [HttpPost]
        public JsonResult ToggleServerIpStatus(int id)
        {
            // Toggle the active/inactive status of a server IP
            // to temporarily enable or disable scanning on that server
            var result = _serverIpService.ToggleServerIpStatus(id);

            // If toggle fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
