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
    public class AdminEmployeesController : BaseController
    {
        private readonly IEmployeeService _employeeService;

        public AdminEmployeesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _employeeService = new EmployeeService(unitOfWork);
        }

        // GET: AdminEmployees/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Employees.cshtml");
        }

        // GET: AdminEmployees/GetEmployees
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

        // GET: AdminEmployees/GetEmployee
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

        // POST: AdminEmployees/CreateEmployee
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

        // POST: AdminEmployees/UpdateEmployee
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

        // POST: AdminEmployees/DeleteEmployee
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

        // POST: AdminEmployees/ToggleEmployeeStatus
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
    }
}
