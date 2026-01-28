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
    /// <summary>
    /// Manages branch reprocessing operations for handling attendance sync issues,
    /// including identifying problematic branches and triggering reprocessing.
    /// </summary>
    [AuthorizeUser]
    public class BranchReprocessController : BaseController
    {
        /// <summary>
        /// Branch issue service for identifying and reprocessing problematic branches.
        /// </summary>
        private readonly IBranchIssueService _service;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public BranchReprocessController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            var repository = new BranchIssueRepository();
            _service = new BranchIssueService(unitOfWork, repository);
        }

        // GET: BranchReprocess/Index
        public ActionResult Index()
        {
            // Return the branch reprocessing view
            return View("~/Views/BranchReprocess/Index.cshtml");
        }

        // GET: BranchReprocess/GetServerIps
        [HttpGet]
        public JsonResult GetServerIps()
        {
            // Retrieve list of available database server IPs
            var result = _service.GetAllServerIps();

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return list of server IPs
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: BranchReprocess/GetDatabases
        [HttpGet]
        public JsonResult GetDatabases(int serverIpId)
        {
            // Retrieve databases available on the specified server
            var result = _service.GetDatabasesForServer(serverIpId);

            // If retrieval fails, return error response
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return list of databases
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // GET: BranchReprocess/GetLastMonth
        [HttpGet]
        public JsonResult GetLastMonth(int serverIpId, string databaseName)
        {
            // Retrieve the last processed month date for the specified database
            var result = _service.GetLastMonthDate(serverIpId, databaseName);

            // If retrieval fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            // Return last month date
            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // POST: BranchReprocess/LoadProblemBranches
        [HttpPost]
        public JsonResult LoadProblemBranches(BranchIssueRequestDto request)
        {
            // Identify branches with attendance sync problems for the specified criteria
            var result = _service.GetProblemBranches(request.ServerIpId, request.DatabaseName, request.MonthStartDate, request.LocationId);

            // If identification fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return list of problematic branches
            return Json(ApiResponse<object>.Success(result.Data));
        }

        // POST: BranchReprocess/ReprocessBranch
        [HttpPost]
        public JsonResult ReprocessBranch(ReprocessBranchRequestDto request)
        {
            // Trigger reprocessing for the specified branch to fix attendance data
            var result = _service.ReprocessBranch(request);

            // If reprocessing fails, return error
            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message));
            }

            // Return reprocessing result with success message
            return Json(ApiResponse<object>.Success(result.Data, result.Message));
        }
    }
}
