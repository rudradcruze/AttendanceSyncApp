using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;
using AttandanceSyncApp.Services.AttandanceSync;

namespace AttandanceSyncApp.Controllers
{
    [AuthorizeUser]
    public class CompanyRequestController : BaseController
    {
        private readonly ICompanyRequestService _companyRequestService;
        private readonly IAuthUnitOfWork _authUnitOfWork;

        public CompanyRequestController() : base()
        {
            _authUnitOfWork = new AuthUnitOfWork();
            _companyRequestService = new CompanyRequestService(_authUnitOfWork);
        }

        public CompanyRequestController(ICompanyRequestService companyRequestService, IAuthUnitOfWork unitOfWork)
            : base()
        {
            _companyRequestService = companyRequestService;
            _authUnitOfWork = unitOfWork;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsAdmin)
            {
                ViewBag.Message = "Administrators cannot access the User Company Requests page.";
                filterContext.Result = View("AccessDenied");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: CompanyRequest/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: CompanyRequest/GetMyRequests
        [HttpGet]
        public JsonResult GetMyRequests(int page = 1, int pageSize = 20)
        {
            var result = _companyRequestService.GetUserRequestsPaged(CurrentUserId, page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<PagedResultDto<CompanyRequestDto>>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<PagedResultDto<CompanyRequestDto>>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: CompanyRequest/CreateRequest
        [HttpPost]
        public JsonResult CreateRequest(CompanyRequestCreateDto dto)
        {
            var sessionId = CurrentSessionId;

            if (CurrentUser == null || sessionId == 0)
            {
                return Json(ApiResponse<int>.Fail("Session expired"));
            }

            var result = _companyRequestService.CreateRequest(dto, CurrentUserId, sessionId);

            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message));
            }

            return Json(ApiResponse<int>.Success(result.Data, result.Message));
        }

        // POST: CompanyRequest/CancelRequest
        [HttpPost]
        public JsonResult CancelRequest(int id)
        {
            var result = _companyRequestService.CancelRequest(id, CurrentUserId);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // GET: CompanyRequest/GetEmployees
        [HttpGet]
        public JsonResult GetEmployees()
        {
            var result = _companyRequestService.GetActiveEmployees();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var data = result.Data.Select(e => new { e.Id, e.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: CompanyRequest/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies()
        {
            var result = _companyRequestService.GetActiveCompanies();

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            var data = result.Data.Select(c => new { c.Id, c.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: CompanyRequest/GetTools
        [HttpGet]
        public JsonResult GetTools()
        {
            var result = _companyRequestService.GetActiveTools();

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
