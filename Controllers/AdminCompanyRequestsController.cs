using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Manages company database access requests for administrators,
    /// including approval, rejection, and database assignment operations.
    /// </summary>
    [AdminAuthorize]
    public class AdminCompanyRequestsController : BaseController
    {
        /// <summary>
        /// Admin company request service for handling request operations.
        /// </summary>
        private readonly IAdminCompanyRequestService _adminCompanyRequestService;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminCompanyRequestsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminCompanyRequestService = new AdminCompanyRequestService(unitOfWork);
        }

        // GET: AdminCompanyRequests/Index
        public ActionResult Index()
        {
            // Return the company requests management view
            return View("~/Views/Admin/CompanyRequests.cshtml");
        }

        // GET: AdminCompanyRequests/GetAllCompanyRequests
        [HttpGet]
        public JsonResult GetAllCompanyRequests(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of company requests
            var result = _adminCompanyRequestService.GetAllRequestsPaged(page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return request data with pagination info
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanyRequests/GetCompanyRequest
        [HttpGet]
        public JsonResult GetCompanyRequest(int id)
        {
            // Retrieve specific company request details by ID
            var result = _adminCompanyRequestService.GetRequestById(id);

            // If request not found or error occurs, return failure
            if (!result.Success)
            {
                return Json(ApiResponse<CompanyRequestListDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return request details
            return Json(ApiResponse<CompanyRequestListDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: AdminCompanyRequests/UpdateCompanyRequestStatus
        [HttpPost]
        public JsonResult UpdateCompanyRequestStatus(int requestId, string status)
        {
            // Update the status of a company request
            var result = _adminCompanyRequestService.UpdateRequestStatus(requestId, status);

            // If update fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/AcceptRequest
        [HttpPost]
        public JsonResult AcceptRequest(int requestId)
        {
            // Accept and approve a company request
            var result = _adminCompanyRequestService.AcceptRequest(requestId);

            // If acceptance fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/RejectRequest
        [HttpPost]
        public JsonResult RejectRequest(int requestId)
        {
            // Reject a company request
            var result = _adminCompanyRequestService.RejectRequest(requestId);

            // If rejection fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/AssignDatabase
        [HttpPost]
        public JsonResult AssignDatabase(int requestId)
        {
            // Assign database configuration to a company request
            var result = _adminCompanyRequestService.AssignDatabase(requestId, CurrentUserId);

            // If assignment fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminCompanyRequests/GetDatabaseConfigForRequest
        [HttpGet]
        public JsonResult GetDatabaseConfigForRequest(int requestId)
        {
            // Retrieve database configuration associated with a request
            var result = _adminCompanyRequestService.GetDatabaseConfigForRequest(requestId);

            // If configuration not found, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return database configuration details
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanyRequests/CheckForNewRequests
        [HttpGet]
        public JsonResult CheckForNewRequests(int lastKnownId)
        {
            // Check for new requests created after the last known ID
            var result = _adminCompanyRequestService.GetNewRequestsCount(lastKnownId);

            // If check fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return count of new requests
            return Json(ApiResponse<int>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanyRequests/GetNewestRequestId
        [HttpGet]
        public JsonResult GetNewestRequestId()
        {
            // Retrieve the ID of the most recently created request
            var result = _adminCompanyRequestService.GetNewestRequestId();

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return newest request ID
            return Json(ApiResponse<int>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }
    }
}
