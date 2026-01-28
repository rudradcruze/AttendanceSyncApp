using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Manages database assignments for company requests,
    /// including viewing, revoking, and restoring assignments.
    /// </summary>
    [AdminAuthorize]
    public class AdminDatabaseAssignmentsController : BaseController
    {
        /// <summary>
        /// Database assignment service for managing assignments.
        /// </summary>
        private readonly IAdminDatabaseAssignmentService _assignmentService;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminDatabaseAssignmentsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _assignmentService = new AdminDatabaseAssignmentService(unitOfWork);
        }

        // GET: AdminDatabaseAssignments/Index
        public ActionResult Index()
        {
            // Return the database assignments management view
            return View("~/Views/Admin/DatabaseAssignments.cshtml");
        }

        // GET: AdminDatabaseAssignments/GetAll
        [HttpGet]
        public JsonResult GetAll(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of all database assignments
            var result = _assignmentService.GetAllAssignmentsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return assignment data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseAssignments/Get
        [HttpGet]
        public JsonResult Get(int id)
        {
            // Retrieve specific database assignment details by ID
            var result = _assignmentService.GetAssignmentById(id);

            // If assignment not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return assignment details
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminDatabaseAssignments/GetByRequest
        [HttpGet]
        public JsonResult GetByRequest(int companyRequestId)
        {
            // Retrieve database assignment for a specific company request
            var result = _assignmentService.GetAssignmentByRequestId(companyRequestId);

            // If assignment not found, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return assignment details
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminDatabaseAssignments/RevokeAssignment
        [HttpPost]
        public JsonResult RevokeAssignment(int id)
        {
            // Revoke database access for the specified assignment
            var result = _assignmentService.RevokeAssignment(id);

            // If revocation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminDatabaseAssignments/UnrevokeAssignment
        [HttpPost]
        public JsonResult UnrevokeAssignment(int id)
        {
            // Restore database access for a previously revoked assignment
            var result = _assignmentService.UnrevokeAssignment(id);

            // If restoration fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }
    }
}
