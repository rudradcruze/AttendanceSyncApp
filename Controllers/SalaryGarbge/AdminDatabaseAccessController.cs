using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.AttandanceSync;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.SalaryGarbge;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Controllers.SalaryGarbge
{
    /// <summary>
    /// Manages database access permissions for salary garbage scanning operations.
    /// Allows administrators to control which databases on each server can be scanned
    /// for problematic salary records.
    /// </summary>
    [AdminAuthorize]
    public class AdminDatabaseAccessController : BaseController
    {
        /// Service for managing database access permissions.
        private readonly IDatabaseAccessService _dbAccessService;

        /// Service for managing server IP configurations.
        private readonly IServerIpManagementService _serverIpService;

        /// Initializes controller with default services.
        public AdminDatabaseAccessController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _dbAccessService = new DatabaseAccessService(unitOfWork);
            _serverIpService = new ServerIpManagementService(unitOfWork);
        }

        // GET: AdminDatabaseAccess/Index
        public ActionResult Index()
        {
            // Return the database access management view
            return View("~/Views/Admin/SalaryGarbge/DatabaseAccess.cshtml");
        }

        // GET: AdminDatabaseAccess/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps()
        {
            // Retrieve all server IPs with pagination
            var result = _serverIpService.GetServerIpsPaged(1, 1000);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Filter to only active servers for database access management
            var activeServers = result.Data.Data.Where(s => s.IsActive).ToList();
            return Json(ApiResponse<object>.Success(activeServers), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseAccess/GetDatabasesWithAccess
        [HttpGet]
        public JsonResult GetDatabasesWithAccess(int serverIpId)
        {
            // Retrieve all databases on the specified server
            // along with their access status for scanning
            var result = _dbAccessService.GetDatabasesWithAccessStatus(serverIpId);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return databases with their access permissions
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminDatabaseAccess/AddDatabaseAccess
        [HttpPost]
        public JsonResult AddDatabaseAccess(int serverIpId, string databaseName)
        {
            // Grant access permission for the specified database
            // to be included in salary garbage scans
            var result = _dbAccessService.AddDatabaseAccess(serverIpId, databaseName);

            // If adding access fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseAccess/UpdateDatabaseAccess
        [HttpPost]
        public JsonResult UpdateDatabaseAccess(int serverIpId, string databaseName, bool hasAccess)
        {
            // Update access permission for an existing database
            // to enable or disable it from salary garbage scans
            var result = _dbAccessService.UpdateDatabaseAccess(serverIpId, databaseName, hasAccess);

            // If update fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseAccess/RemoveDatabaseAccess
        [HttpPost]
        public JsonResult RemoveDatabaseAccess(int serverIpId, string databaseName)
        {
            // Remove access permission for the specified database,
            // preventing it from being scanned for salary garbage
            var result = _dbAccessService.RemoveDatabaseAccess(serverIpId, databaseName);

            // If removal fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success response
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
