using System.Data.SqlClient;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Manages attendance sync requests for administrators,
    /// including filtering, processing, and database assignment operations.
    /// </summary>
    [AdminAuthorize]
    public class AdminRequestsController : BaseController
    {
        /// <summary>
        /// Admin request service for handling sync request operations.
        /// </summary>
        private readonly IAdminRequestService _adminRequestService;

        /// <summary>
        /// Database assignment service for managing database configurations.
        /// </summary>
        private readonly IDatabaseAssignmentService _dbAssignmentService;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminRequestsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminRequestService = new AdminRequestService(unitOfWork);
            _dbAssignmentService = new DatabaseAssignmentService(unitOfWork);
        }

        // GET: AdminRequests/SyncRequests
        public ActionResult SyncRequests()
        {
            // Return the sync requests management view
            return View("~/Views/Admin/SyncRequests.cshtml");
        }

        // GET: AdminRequests/GetAllRequests
        [HttpGet]
        public JsonResult GetAllRequests(RequestFilterDto filter)
        {
            // Retrieve filtered list of sync requests based on filter criteria
            var result = _adminRequestService.GetRequestsFiltered(filter);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return filtered request data
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/ProcessRequest
        [HttpPost]
        public JsonResult ProcessRequest(ProcessRequestDto dto)
        {
            // Process a sync request with external sync ID and success status
            var result = _adminRequestService.ProcessRequest(dto.RequestId, dto.ExternalSyncId, dto.IsSuccessful);

            // If processing fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminRequests/GetRequest
        [HttpGet]
        public JsonResult GetRequest(int id)
        {
            // Retrieve specific sync request details by ID
            var result = _adminRequestService.GetRequestById(id);

            // If request not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<RequestListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return request details
            return Json(ApiResponse<RequestListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/AssignDatabase
        [HttpPost]
        public JsonResult AssignDatabase(AssignDatabaseDto dto)
        {
            // Assign database configuration to a sync request
            var result = _dbAssignmentService.AssignDatabase(dto, CurrentUserId);

            // If assignment fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminRequests/GetDatabaseAssignment
        [HttpGet]
        public JsonResult GetDatabaseAssignment(int requestId)
        {
            // Retrieve database assignment for a specific request
            var result = _dbAssignmentService.GetAssignment(requestId);

            // If assignment not found, return error
            if (!result.Success)
            {
                return Json(ApiResponse<AssignDatabaseDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return assignment details
            return Json(ApiResponse<AssignDatabaseDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/TestDatabaseConnection
        [HttpPost]
        public JsonResult TestDatabaseConnection(AssignDatabaseDto dto)
        {
            try
            {
                // Create database connection DTO from assignment data
                var connectionDto = new DatabaseConnectionDto
                {
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseUserId = dto.DatabaseUserId,
                    DatabasePassword = dto.DatabasePassword,
                    DatabaseName = dto.DatabaseName
                };

                // Build connection string from configuration
                var connectionString = BuildConnectionString(connectionDto);

                // Attempt to open connection to test validity
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return Json(ApiResponse.Success("Connection successful"));
                }
            }
            catch (System.Exception ex)
            {
                // Return error if connection test fails
                return Json(ApiResponse.Fail($"Connection failed: {ex.Message}"));
            }
        }

        // POST: AdminRequests/UpdateRequestStatus
        [HttpPost]
        public JsonResult UpdateRequestStatus(int requestId, string status)
        {
            // Update the status of a sync request
            var result = _adminRequestService.UpdateRequestStatus(requestId, status);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        /// <summary>
        /// Builds a SQL Server connection string from database configuration.
        /// </summary>
        /// <param name="config">Database connection configuration.</param>
        /// <returns>Formatted SQL connection string.</returns>
        private string BuildConnectionString(DatabaseConnectionDto config)
        {
            // Create connection string builder with provided parameters
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.DatabaseIP,
                InitialCatalog = config.DatabaseName,
                UserID = config.DatabaseUserId,
                Password = config.DatabasePassword,
                IntegratedSecurity = false,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
    }
}