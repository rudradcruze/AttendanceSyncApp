using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminDashboardController : BaseController
    {
        public AdminDashboardController() : base()
        {
        }

        // GET: AdminDashboard/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Dashboard.cshtml");
        }

        // GET: AdminDashboard/GetStats
        [HttpGet]
        public JsonResult GetStats()
        {
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var totalUsers = unitOfWork.Users.Count();
                var totalRequests = unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var totalEmployees = unitOfWork.Employees.Count();
                var totalCompanies = unitOfWork.SyncCompanies.Count();
                var totalTools = unitOfWork.Tools.Count();

                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalRequests = totalRequests,
                    TotalEmployees = totalEmployees,
                    TotalCompanies = totalCompanies,
                    TotalTools = totalTools
                };

                return Json(ApiResponse<object>.Success(stats), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
