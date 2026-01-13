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
    [AdminAuthorize]
    public class AdminController : BaseController
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IAdminRequestService _adminRequestService;
        private readonly IDatabaseAssignmentService _dbAssignmentService;

        public AdminController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminUserService = new AdminUserService(unitOfWork);
            _adminRequestService = new AdminRequestService(unitOfWork);
            _dbAssignmentService = new DatabaseAssignmentService(unitOfWork);
        }

        public AdminController(
            IAdminUserService adminUserService,
            IAdminRequestService adminRequestService,
            IDatabaseAssignmentService dbAssignmentService)
            : base()
        {
            _adminUserService = adminUserService;
            _adminRequestService = adminRequestService;
            _dbAssignmentService = dbAssignmentService;
        }

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Admin/Users
        public ActionResult Users()
        {
            return View();
        }

        // GET: Admin/Requests
        public ActionResult Requests()
        {
            return View();
        }

        // GET: Admin/Companies
        public ActionResult Companies()
        {
            return View();
        }

        // GET: Admin/Tools
        public ActionResult Tools()
        {
            return View();
        }

        // GET: Admin/GetUsers
        [HttpGet]
        public JsonResult GetUsers(int page = 1, int pageSize = 20)
        {
            var result = _adminUserService.GetUsersPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/GetUser
        [HttpGet]
        public JsonResult GetUser(int id)
        {
            var result = _adminUserService.GetUserById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<UserListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<UserListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/UpdateUser
        [HttpPost]
        public JsonResult UpdateUser(UserListDto userDto)
        {
            var result = _adminUserService.UpdateUser(userDto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public JsonResult ToggleUserStatus(int userId)
        {
            var result = _adminUserService.ToggleUserStatus(userId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // GET: Admin/GetAllRequests
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

        // GET: Admin/GetRequest
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

        // POST: Admin/AssignDatabase
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

        // GET: Admin/GetDatabaseAssignment
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

        // POST: Admin/TestDatabaseConnection
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

        // POST: Admin/UpdateRequestStatus
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

        // GET: Admin/GetStats
        [HttpGet]
        public JsonResult GetStats()
        {
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var totalUsers = unitOfWork.Users.Count();
                var totalRequests = unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var pendingRequests = unitOfWork.AttandanceSyncRequests.Count(r => r.Status == "NR" || r.Status == "IP");
                var completedRequests = unitOfWork.AttandanceSyncRequests.Count(r => r.Status == "CP");

                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalRequests = totalRequests,
                    PendingRequests = pendingRequests,
                    CompletedRequests = completedRequests
                };

                return Json(ApiResponse<object>.Success(stats), JsonRequestBehavior.AllowGet);
            }
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
    }
}
