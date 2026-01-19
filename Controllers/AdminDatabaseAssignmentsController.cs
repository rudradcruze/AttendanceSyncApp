using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminDatabaseAssignmentsController : BaseController
    {
        private readonly IAdminDatabaseAssignmentService _assignmentService;

        public AdminDatabaseAssignmentsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _assignmentService = new AdminDatabaseAssignmentService(unitOfWork);
        }

        // GET: AdminDatabaseAssignments/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/DatabaseAssignments.cshtml");
        }

        // GET: AdminDatabaseAssignments/GetAll
        [HttpGet]
        public JsonResult GetAll(int page = 1, int pageSize = 20)
        {
            var result = _assignmentService.GetAllAssignmentsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseAssignments/Get
        [HttpGet]
        public JsonResult Get(int id)
        {
            var result = _assignmentService.GetAssignmentById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseAssignments/GetByRequest
        [HttpGet]
        public JsonResult GetByRequest(int companyRequestId)
        {
            var result = _assignmentService.GetAssignmentByRequestId(companyRequestId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminDatabaseAssignments/RevokeAssignment
        [HttpPost]
        public JsonResult RevokeAssignment(int id)
        {
            var result = _assignmentService.RevokeAssignment(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseAssignments/UnrevokeAssignment
        [HttpPost]
        public JsonResult UnrevokeAssignment(int id)
        {
            var result = _assignmentService.UnrevokeAssignment(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }
    }
}
