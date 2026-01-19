using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;
using AttandanceSyncApp.Services.Sync;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Controller for Attandance Synchronization - User-facing dashboard
    /// </summary>
    [AuthorizeUser]
    public class AttandanceController : BaseController
    {
        private readonly ISyncRequestService _syncRequestService;
        private readonly IAuthUnitOfWork _authUnitOfWork;

        public AttandanceController() : base()
        {
            _authUnitOfWork = new AuthUnitOfWork();
            _syncRequestService = new SyncRequestService(_authUnitOfWork);
        }

        public AttandanceController(ISyncRequestService syncRequestService, IAuthUnitOfWork unitOfWork)
            : base()
        {
            _syncRequestService = syncRequestService;
            _authUnitOfWork = unitOfWork;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsAdmin)
            {
                if (filterContext.ActionDescriptor.ActionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase))
                {
                    filterContext.Result = new RedirectResult("~/AdminDashboard");
                    return;
                }

                ViewBag.Message = "Administrators cannot access the User Dashboard.";
                filterContext.Result = View("AccessDenied");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Attandance/Index - Main dashboard
        public ActionResult Index()
        {
            return View();
        }

        // GET: Attandance/Requests - User's requests page (CompanyRequest)
        public ActionResult Requests()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMyRequests(int page = 1, int pageSize = 20)
        {
            var result = _syncRequestService.GetUserRequestsPaged(CurrentUserId, page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<PagedResultDto<SyncRequestDto>>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<PagedResultDto<SyncRequestDto>>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRequest(SyncRequestCreateDto dto)
        {
            var sessionId = CurrentSessionId;

            if (CurrentUser == null || sessionId == 0)
            {
                return Json(ApiResponse<int>.Fail("Session expired"));
            }

            var result = _syncRequestService.CreateSyncRequest(dto, CurrentUserId, sessionId);

            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message));
            }

            return Json(ApiResponse<int>.Success(result.Data, result.Message));
        }

        [HttpPost]
        public JsonResult CancelRequest(int id)
        {
            var result = _syncRequestService.CancelSyncRequest(id, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        [HttpPost]
        public JsonResult GetStatusesByIds(int[] ids)
        {
            var result = _syncRequestService.GetStatusesByIds(ids);

            if (!result.Success)
            {
                return Json(ApiResponse<IEnumerable<StatusDto>>.Fail(result.Message));
            }

            return Json(ApiResponse<IEnumerable<StatusDto>>.Success(result.Data));
        }

        [HttpGet]
        public JsonResult GetEmployees()
        {
            var result = _syncRequestService.GetActiveEmployees();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var data = result.Data.Select(e => new { e.Id, e.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCompanies()
        {
            var result = _syncRequestService.GetActiveCompanies();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var data = result.Data.Select(c => new { c.Id, c.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTools()
        {
            var result = _syncRequestService.GetActiveTools();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var data = result.Data.Select(t => new { t.Id, t.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _authUnitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
