using System.Web.Mvc;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services;
using AttandanceSyncApp.Services.Interfaces;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Controller for Attandance Synchronization - Thin layer handling HTTP requests/responses only
    /// </summary>
    public class AttandanceController : Controller
    {
        private readonly IAttandanceSynchronizationService _synchronizationService;
        private readonly IUnitOfWork _unitOfWork;

        public AttandanceController()
        {
            _unitOfWork = new UnitOfWork();
            var companyService = new CompanyService(_unitOfWork);
            _synchronizationService = new AttandanceSynchronizationService(_unitOfWork, companyService);
        }

        public AttandanceController(IAttandanceSynchronizationService synchronizationService, IUnitOfWork unitOfWork)
        {
            _synchronizationService = synchronizationService;
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSynchronizationsPaged(int page = 1, int pageSize = 20)
        {
            var result = _synchronizationService.GetSynchronizationsPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(new { error = true, message = result.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                totalRecords = result.Data.TotalRecords,
                page = result.Data.Page,
                pageSize = result.Data.PageSize,
                data = result.Data.Data
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSynchronization(string fromDate, string toDate)
        {
            var result = _synchronizationService.CreateSynchronization(fromDate, toDate);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public JsonResult GetStatusesByIds(int[] ids)
        {
            var result = _synchronizationService.GetStatusesByIds(ids);

            if (!result.Success)
            {
                return Json(new { error = true, message = result.Message });
            }

            return Json(result.Data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
