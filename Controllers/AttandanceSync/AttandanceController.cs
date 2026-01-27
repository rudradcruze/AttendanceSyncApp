using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.AttandanceSync;

namespace AttandanceSyncApp.Controllers.AttandanceSync
{
    /// <summary>
    /// Controller for Attandance Synchronization - User-facing dashboard
    /// </summary>
    [AuthorizeUser]
    public class AttandanceController : BaseController
    {
        private readonly ISyncRequestService _syncRequestService;
        private readonly IUserToolService _userToolService;
        private readonly IAuthUnitOfWork _authUnitOfWork;

        public AttandanceController() : base()
        {
            _authUnitOfWork = new AuthUnitOfWork();
            _syncRequestService = new SyncRequestService(_authUnitOfWork);
            _userToolService = new UserToolService(_authUnitOfWork);
        }

        public AttandanceController(ISyncRequestService syncRequestService, IUserToolService userToolService, IAuthUnitOfWork unitOfWork)
            : base()
        {
            _syncRequestService = syncRequestService;
            _userToolService = userToolService;
            _authUnitOfWork = unitOfWork;
        }

        // Check if the user request for any admin action it will redirect to admin dashboard else if the user is admin and request for the user action it will return access denied view.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsAdmin)
            {
                var actionName = filterContext.ActionDescriptor.ActionName;
                if (actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase) ||
                    actionName.Equals("Dashboard", System.StringComparison.OrdinalIgnoreCase))
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

        // GET: Attandance/Dashboard - User dashboard with tool cards
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Attandance/Index - Attendance sync page
        public ActionResult Index()
        {
            return View();
        }

        // GET: Attandance/GetMyTools - Get user's assigned tools
        [HttpGet]
        public JsonResult GetMyTools()
        {
            var result = _userToolService.GetUserAssignedTools(CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse<IEnumerable<AssignedToolDto>>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<IEnumerable<AssignedToolDto>>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Attandance/GetMyCompanyDatabases - Get user's assigned company databases
        [HttpGet]
        public JsonResult GetMyCompanyDatabases()
        {
            var result = _syncRequestService.GetUserCompanyDatabases(CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse<IEnumerable<UserCompanyDatabaseDto>>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<IEnumerable<UserCompanyDatabaseDto>>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: Attandance/Requests - User's requests page (CompanyRequest)
        public ActionResult Requests()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMyRequests(int? companyId, int page = 1, int pageSize = 20, string sortColumn = "ToDate", string sortDirection = "DESC")
        {
            var result = _syncRequestService.GetUserRequestsPaged(CurrentUserId, companyId, page, pageSize, sortColumn, sortDirection);

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
        public JsonResult CreateOnTheFlySynchronization(SyncRequestCreateDto dto)
        {
            var sessionId = CurrentSessionId;

            if (CurrentUser == null || sessionId == 0)
            {
                return Json(ApiResponse<int>.Fail("Session expired"));
            }

            var result = _syncRequestService.CreateOnTheFlySynchronization(dto, CurrentUserId, sessionId);

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

        [HttpPost]
        public JsonResult GetExternalStatusesByIds(int companyId, int[] ids)
        {
            var result = _syncRequestService.GetExternalStatusesByIds(CurrentUserId, companyId, ids);

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
