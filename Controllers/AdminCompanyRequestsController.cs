using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminCompanyRequestsController : BaseController
    {
        private readonly IAdminCompanyRequestService _adminCompanyRequestService;

        public AdminCompanyRequestsController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _adminCompanyRequestService = new AdminCompanyRequestService(unitOfWork);
        }

        // GET: AdminCompanyRequests/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/CompanyRequests.cshtml");
        }

        // GET: AdminCompanyRequests/GetAllCompanyRequests
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

        // GET: AdminCompanyRequests/GetCompanyRequest
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

        // POST: AdminCompanyRequests/UpdateCompanyRequestStatus
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

        // POST: AdminCompanyRequests/AcceptRequest
        [HttpPost]
        public JsonResult AcceptRequest(int requestId)
        {
            var result = _adminCompanyRequestService.AcceptRequest(requestId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/RejectRequest
        [HttpPost]
        public JsonResult RejectRequest(int requestId)
        {
            var result = _adminCompanyRequestService.RejectRequest(requestId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/RevokeConnection
        [HttpPost]
        public JsonResult RevokeConnection(int requestId)
        {
            var result = _adminCompanyRequestService.RevokeConnection(requestId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminCompanyRequests/AssignDatabase
        [HttpPost]
        public JsonResult AssignDatabase(int requestId)
        {
            var result = _adminCompanyRequestService.AssignDatabase(requestId, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // GET: AdminCompanyRequests/GetDatabaseConfigForRequest
        [HttpGet]
        public JsonResult GetDatabaseConfigForRequest(int requestId)
        {
            var result = _adminCompanyRequestService.GetDatabaseConfigForRequest(requestId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanyRequests/CheckForNewRequests
        [HttpGet]
        public JsonResult CheckForNewRequests(int lastKnownId)
        {
            var result = _adminCompanyRequestService.GetNewRequestsCount(lastKnownId);

            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<int>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: AdminCompanyRequests/GetNewestRequestId
        [HttpGet]
        public JsonResult GetNewestRequestId()
        {
            var result = _adminCompanyRequestService.GetNewestRequestId();

            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<int>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }
    }
}
