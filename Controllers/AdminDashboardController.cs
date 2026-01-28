using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Provides dashboard functionality for administrators,
    /// including system statistics and overview data.
    /// </summary>
    [AdminAuthorize]
    public class AdminDashboardController : BaseController
    {
        /// Initializes controller with default services.
        public AdminDashboardController() : base()
        {
        }

        // GET: AdminDashboard/Index
        public ActionResult Index()
        {
            // Return the admin dashboard view
            return View("~/Views/Admin/Dashboard.cshtml");
        }

        // GET: AdminDashboard/GetStats
        [HttpGet]
        public JsonResult GetStats()
        {
            // Retrieve system-wide statistics for dashboard display
            using (var unitOfWork = new AuthUnitOfWork())
            {
                // Gather counts from various repositories
                var totalUsers = unitOfWork.Users.Count();
                var totalRequests = unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var totalEmployees = unitOfWork.Employees.Count();
                var totalCompanies = unitOfWork.SyncCompanies.Count();
                var totalTools = unitOfWork.Tools.Count();

                // Package statistics into a response object
                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalRequests = totalRequests,
                    TotalEmployees = totalEmployees,
                    TotalCompanies = totalCompanies,
                    TotalTools = totalTools
                };

                // Return statistics as JSON for client-side rendering
                return Json(ApiResponse<object>.Success(stats), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
