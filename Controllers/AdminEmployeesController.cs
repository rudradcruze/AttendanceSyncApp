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
    /// Handles employee management operations for administrators,
    /// including CRUD operations and status management.
    /// </summary>
    [AdminAuthorize]
    public class AdminEmployeesController : BaseController
    {
        /// Employee service for business logic.
        private readonly IEmployeeService _employeeService;

        /// Initializes controller with default services.
        public AdminEmployeesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _employeeService = new EmployeeService(unitOfWork);
        }

        // GET: AdminEmployees/Index
        public ActionResult Index()
        {
            // Return the employee management view
            return View("~/Views/Admin/Employees.cshtml");
        }

        // GET: AdminEmployees/GetEmployees
        [HttpGet]
        public JsonResult GetEmployees(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of employees
            var result = _employeeService.GetEmployeesPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return employee data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminEmployees/GetEmployee
        [HttpGet]
        public JsonResult GetEmployee(int id)
        {
            // Retrieve specific employee details by ID
            var result = _employeeService.GetEmployeeById(id);

            // If employee not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<EmployeeDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return employee details
            return Json(ApiResponse<EmployeeDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminEmployees/CreateEmployee
        [HttpPost]
        public JsonResult CreateEmployee(EmployeeCreateDto dto)
        {
            // Attempt to create a new employee
            var result = _employeeService.CreateEmployee(dto);

            // If creation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminEmployees/UpdateEmployee
        [HttpPost]
        public JsonResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            // Attempt to update existing employee information
            var result = _employeeService.UpdateEmployee(dto);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminEmployees/DeleteEmployee
        [HttpPost]
        public JsonResult DeleteEmployee(int id)
        {
            // Attempt to delete the specified employee
            var result = _employeeService.DeleteEmployee(id);

            // If deletion fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminEmployees/ToggleEmployeeStatus
        [HttpPost]
        public JsonResult ToggleEmployeeStatus(int id)
        {
            // Toggle employee active/inactive status
            var result = _employeeService.ToggleEmployeeStatus(id);

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
