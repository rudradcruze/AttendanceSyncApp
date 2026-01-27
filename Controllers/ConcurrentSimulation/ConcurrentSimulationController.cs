using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.ConcurrentSimulation;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.ConcurrentSimulation;
using AttandanceSyncApp.Services.Interfaces.Admin;
using AttandanceSyncApp.Services.Interfaces.ConcurrentSimulation;

namespace AttandanceSyncApp.Controllers.ConcurrentSimulation
{
    [AuthorizeUser]
    public class ConcurrentSimulationController : BaseController
    {
        private readonly IConcurrentSimulationService _service;
        private readonly IUserToolService _userToolService;
        private readonly IAuthUnitOfWork _unitOfWork;

        public ConcurrentSimulationController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _unitOfWork = new AuthUnitOfWork();
            _userToolService = new UserToolService(_unitOfWork);
            _service = new ConcurrentSimulationService(unitOfWork);
        }

        // Check if the user request for any admin action it will redirect to admin dashboard else if the user is admin and request for the user action it will return access denied view or redirect to the '/'.
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

        // GET: ConcurrentSimulation/Index
        public ActionResult Index()
        {
            return View("~/Views/ConcurrentSimulation/Index.cshtml");
        }

        // GET: ConcurrentSimulation/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps()
        {
            var result = _service.GetAllServerIps();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: ConcurrentSimulation/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            var result = _service.GetDatabasesForServer(serverIpId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: ConcurrentSimulation/GetPeriodEndData
        [HttpGet]
        public JsonResult GetPeriodEndData(int serverIpId, string databaseName)
        {
            var result = _service.GetPeriodEndData(serverIpId, databaseName);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: ConcurrentSimulation/HitConcurrent
        [HttpPost]
        public JsonResult HitConcurrent(HitConcurrentRequestDto request)
        {
            try
            {
                var result = _service.HitConcurrent(request);

                if (!result.Success)
                {
                    return Json(ApiResponse<HitConcurrentResponseDto>.Fail(result.Message));
                }

                return Json(ApiResponse<HitConcurrentResponseDto>.Success(result.Data, result.Message));
            }
            catch (System.Exception ex)
            {
                return Json(ApiResponse<HitConcurrentResponseDto>.Fail($"Error: {ex.Message}. Inner: {ex.InnerException?.Message}"));
            }
        }
    }
}
