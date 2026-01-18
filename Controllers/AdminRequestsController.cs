using System.Data.SqlClient;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminRequestsController : BaseController
    {
        private readonly IAdminRequestService _adminRequestService;
        private readonly IDatabaseAssignmentService _dbAssignmentService;
        private readonly IAdminCompanyRequestService _adminCompanyRequestService;

        public AdminRequestsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminRequestService = new AdminRequestService(unitOfWork);
            _dbAssignmentService = new DatabaseAssignmentService(unitOfWork);
            _adminCompanyRequestService = new AdminCompanyRequestService(unitOfWork);
        }

        // GET: AdminRequests/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Requests.cshtml");
        }

        // GET: AdminRequests/GetAllRequests
        [HttpGet]
        public JsonResult GetAllRequests(int page = 1, int pageSize = 20)
        {
            var result = _adminRequestService.GetAllRequestsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminRequests/GetRequest
        [HttpGet]
        public JsonResult GetRequest(int id)
        {
            var result = _adminRequestService.GetRequestById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<RequestListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<RequestListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/AssignDatabase
        [HttpPost]
        public JsonResult AssignDatabase(AssignDatabaseDto dto)
        {
            var result = _dbAssignmentService.AssignDatabase(dto, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminRequests/GetDatabaseAssignment
        [HttpGet]
        public JsonResult GetDatabaseAssignment(int requestId)
        {
            var result = _dbAssignmentService.GetAssignment(requestId);

            if (!result.Success)
            {
                return Json(ApiResponse<AssignDatabaseDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<AssignDatabaseDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/TestDatabaseConnection
        [HttpPost]
        public JsonResult TestDatabaseConnection(AssignDatabaseDto dto)
        {
            try
            {
                var connectionDto = new DatabaseConnectionDto
                {
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseUserId = dto.DatabaseUserId,
                    DatabasePassword = dto.DatabasePassword,
                    DatabaseName = dto.DatabaseName
                };

                var connectionString = BuildConnectionString(connectionDto);

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return Json(ApiResponse.Success("Connection successful"));
                }
            }
            catch (System.Exception ex)
            {
                return Json(ApiResponse.Fail($"Connection failed: {ex.Message}"));
            }
        }

        // POST: AdminRequests/UpdateRequestStatus
        [HttpPost]
        public JsonResult UpdateRequestStatus(int requestId, string status)
        {
            var result = _adminRequestService.UpdateRequestStatus(requestId, status);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        private string BuildConnectionString(DatabaseConnectionDto config)
        {
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

        #region Company Requests

        // GET: AdminRequests/GetAllCompanyRequests
        [HttpGet]
        public JsonResult GetAllCompanyRequests(int page = 1, int pageSize = 20)
        {
            var result = _adminCompanyRequestService.GetAllRequestsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminRequests/GetCompanyRequest
        [HttpGet]
        public JsonResult GetCompanyRequest(int id)
        {
            var result = _adminCompanyRequestService.GetRequestById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<CompanyRequestListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<CompanyRequestListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminRequests/UpdateCompanyRequestStatus
        [HttpPost]
        public JsonResult UpdateCompanyRequestStatus(int requestId, string status)
        {
            var result = _adminCompanyRequestService.UpdateRequestStatus(requestId, status);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        #endregion
    }
}
