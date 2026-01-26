using System.Web.Mvc;
using AttandanceSyncApp.Controllers;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.BranchIssue;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.BranchIssue;
using AttandanceSyncApp.Services.BranchIssue;
using AttandanceSyncApp.Services.Interfaces.BranchIssue;

namespace AttendanceSyncApp.Controllers
{
    [AuthorizeUser]
    public class BranchReprocessController : BaseController
    {
        private readonly IBranchIssueService _service;

        public BranchReprocessController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            var repository = new BranchIssueRepository();
            _service = new BranchIssueService(unitOfWork, repository);
        }

        // GET: BranchReprocess/Index
        public ActionResult Index()
        {
            return View("~/Views/BranchReprocess/Index.cshtml");
        }

        // GET: BranchReprocess/GetServerIps
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

        // GET: BranchReprocess/GetDatabases
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

        // GET: BranchReprocess/GetLastMonth
        [HttpGet]
        public JsonResult GetLastMonth(int serverIpId, string databaseName)
        {
            var result = _service.GetLastMonthDate(serverIpId, databaseName);
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: BranchReprocess/LoadProblemBranches
        [HttpPost]
        public JsonResult LoadProblemBranches(BranchIssueRequestDto request)
        {
            var result = _service.GetProblemBranches(request.ServerIpId, request.DatabaseName, request.MonthStartDate, request.LocationId);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: BranchReprocess/ReprocessBranch
        [HttpPost]
        public JsonResult ReprocessBranch(ReprocessBranchRequestDto request)
        {
            var result = _service.ReprocessBranch(request);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            return Json(ApiResponse<object>.Success(result.Data, result.Message));
        }
    }
}
