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
    /// <summary>
    /// Manages company database access requests submitted by users,
    /// including creation, cancellation, and tracking of requests.
    /// </summary>
    [AuthorizeUser]
    public class CompanyRequestController : BaseController
    {
        /// Company request service for business logic.
        private readonly ICompanyRequestService _companyRequestService;

        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _authUnitOfWork;

        /// Initializes controller with default services.
        public CompanyRequestController() : base()
        {
            _authUnitOfWork = new AuthUnitOfWork();
            _companyRequestService = new CompanyRequestService(_authUnitOfWork);
        }

        /// Initializes controller with injected services for testing.
        public CompanyRequestController(ICompanyRequestService companyRequestService, IAuthUnitOfWork unitOfWork)
            : base()
        {
            _companyRequestService = companyRequestService;
            _authUnitOfWork = unitOfWork;
        }

        /// <summary>
        /// Prevents administrators from accessing user company request pages.
        /// </summary>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Block admins from accessing user-specific company request features
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
            // Return the company request management view
            return View();
        }

        // GET: CompanyRequest/GetMyRequests
        [HttpGet]
        public JsonResult GetMyRequests(int page = 1, int pageSize = 20)
        {
            // Retrieve paginated list of current user's company requests
            var result = _companyRequestService.GetUserRequestsPaged(CurrentUserId, page, pageSize);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<PagedResultDto<CompanyRequestDto>>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return request data with pagination info
            return Json(ApiResponse<PagedResultDto<CompanyRequestDto>>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: CompanyRequest/CreateRequest
        [HttpPost]
        public JsonResult CreateRequest(CompanyRequestCreateDto dto)
        {
            // Retrieve current session ID for audit tracking
            var sessionId = CurrentSessionId;

            // Validate user session before creating request
            if (CurrentUser == null || sessionId == 0)
            {
                return Json(ApiResponse<int>.Fail("Session expired"));
            }

            // Attempt to create a new company access request
            var result = _companyRequestService.CreateRequest(dto, CurrentUserId, sessionId);

            // If creation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<int>.Fail(result.Message));
            }

            // Return success with new request ID
            return Json(ApiResponse<int>.Success(result.Data, result.Message));
        }

        // POST: CompanyRequest/CancelRequest
        [HttpPost]
        public JsonResult CancelRequest(int id)
        {
            // Attempt to cancel the specified company request
            var result = _companyRequestService.CancelRequest(id, CurrentUserId);

            // If cancellation fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            // Return success message
            return Json(ApiResponse.Success(result.Message));
        }

        // GET: CompanyRequest/GetEmployees
        [HttpGet]
        public JsonResult GetEmployees()
        {
            // Retrieve active employees for dropdown selection
            var result = _companyRequestService.GetActiveEmployees();

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return simplified employee list with ID and Name only
            var data = result.Data.Select(e => new { e.Id, e.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: CompanyRequest/GetCompanies
        [HttpGet]
        public JsonResult GetCompanies()
        {
            // Retrieve active companies for dropdown selection
            var result = _companyRequestService.GetActiveCompanies();

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return simplified company list with ID and Name only
            var data = result.Data.Select(c => new { c.Id, c.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        // GET: CompanyRequest/GetTools
        [HttpGet]
        public JsonResult GetTools()
        {
            // Retrieve active tools for dropdown selection
            var result = _companyRequestService.GetActiveTools();

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return simplified tool list with ID and Name only
            var data = result.Data.Select(t => new { t.Id, t.Name }).ToList();
            return Json(ApiResponse<object>.Success(data), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Disposes unit of work resources when controller is disposed.
        /// </summary>
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
