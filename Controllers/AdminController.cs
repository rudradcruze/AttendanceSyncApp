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
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyManagementService _companyService;
        private readonly IToolManagementService _toolService;

        public AdminController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminUserService = new AdminUserService(unitOfWork);
            _adminRequestService = new AdminRequestService(unitOfWork);
            _dbAssignmentService = new DatabaseAssignmentService(unitOfWork);
            _employeeService = new EmployeeService(unitOfWork);
            _companyService = new CompanyManagementService(unitOfWork);
            _toolService = new ToolManagementService(unitOfWork);
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

        // GET: Admin/Employees
        public ActionResult Employees()
        {
            return View();
        }

        #region User Management

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

        #endregion

        #region Employee Management

        // GET: Admin/GetEmployees
        [HttpGet]
        public JsonResult GetEmployees(int page = 1, int pageSize = 20)
        {
            var result = _employeeService.GetEmployeesPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/GetEmployee
        [HttpGet]
        public JsonResult GetEmployee(int id)
        {
            var result = _employeeService.GetEmployeeById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<EmployeeDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<EmployeeDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/CreateEmployee
        [HttpPost]
        public JsonResult CreateEmployee(EmployeeCreateDto dto)
        {
            var result = _employeeService.CreateEmployee(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/UpdateEmployee
        [HttpPost]
        public JsonResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            var result = _employeeService.UpdateEmployee(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/DeleteEmployee
        [HttpPost]
        public JsonResult DeleteEmployee(int id)
        {
            var result = _employeeService.DeleteEmployee(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/ToggleEmployeeStatus
        [HttpPost]
        public JsonResult ToggleEmployeeStatus(int id)
        {
            var result = _employeeService.ToggleEmployeeStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        #endregion

        #region Company Management

        // GET: Admin/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies(int page = 1, int pageSize = 20)
        {
            var result = _companyService.GetCompaniesPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/GetCompany
        [HttpGet]
        public JsonResult GetCompany(int id)
        {
            var result = _companyService.GetCompanyById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<CompanyManagementDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<CompanyManagementDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/CreateCompany
        [HttpPost]
        public JsonResult CreateCompany(CompanyCreateDto dto)
        {
            var result = _companyService.CreateCompany(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/UpdateCompany
        [HttpPost]
        public JsonResult UpdateCompany(CompanyUpdateDto dto)
        {
            var result = _companyService.UpdateCompany(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/DeleteCompany
        [HttpPost]
        public JsonResult DeleteCompany(int id)
        {
            var result = _companyService.DeleteCompany(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: Admin/ToggleCompanyStatus
        [HttpPost]
        public JsonResult ToggleCompanyStatus(int id)
        {
            var result = _companyService.ToggleCompanyStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        #endregion

        #region Tool Management

        // GET: Admin/GetTools
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

        // GET: Admin/GetTool
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

        // POST: Admin/CreateTool
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

        // POST: Admin/UpdateTool
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

        // POST: Admin/DeleteTool
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

        // POST: Admin/ToggleToolStatus
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

        #endregion

        #region Request Management

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

        #endregion

        // GET: Admin/GetStats
        [HttpGet]
        public JsonResult GetStats()
        {
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var totalUsers = unitOfWork.Users.Count();
                var totalRequests = unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var totalEmployees = unitOfWork.Employees.Count();
                var totalCompanies = unitOfWork.SyncCompanies.Count();
                var totalTools = unitOfWork.Tools.Count();

                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalRequests = totalRequests,
                    TotalEmployees = totalEmployees,
                    TotalCompanies = totalCompanies,
                    TotalTools = totalTools
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
