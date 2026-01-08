using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using AttandanceSyncApp.Models;

namespace AttandanceSyncApp.Controllers
{
    public class AttandanceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSynchronizationsPaged(int page = 1, int pageSize = 20)
        {
            try
            {
                var query = db.AttandanceSynchronizations
                    .AsNoTracking()
                    .Include(a => a.Company)
                    .OrderByDescending(a => a.Id);

                var totalRecords = query.Count();

                var data = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.FromDate,
                        a.ToDate,
                        CompanyName = a.Company != null ? a.Company.CompanyName : "N/A",
                        a.Status
                    })
                    .ToList();

                return Json(new
                {
                    totalRecords,
                    page,
                    pageSize,
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetSynchronizations()
        {
            try
            {
                var data = db.AttandanceSynchronizations
                    .AsNoTracking()
                    .Include(a => a.Company)
                    .OrderByDescending(a => a.Id)
                    .Select(a => new
                    {
                        a.Id,
                        a.FromDate,
                        a.ToDate,
                        CompanyName = a.Company != null ? a.Company.CompanyName : "N/A",
                        a.Status
                    })
                    .ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateSynchronization(string fromDate, string toDate)
        {
            try
            {
                if (!DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                    return Json(new { success = false, message = "Invalid From Date format" });

                if (!DateTime.TryParse(toDate, out DateTime parsedToDate))
                    return Json(new { success = false, message = "Invalid To Date format" });

                var firstCompany = db.Companies.FirstOrDefault();
                if (firstCompany == null)
                    return Json(new { success = false, message = "No company found in database." });

                var sync = new AttandanceSynchronization
                {
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate,
                    CompanyId = firstCompany.Id,
                    Status = "NR"
                };

                db.AttandanceSynchronizations.Add(sync);
                db.SaveChanges();

                return Json(new { success = true, message = $"Synchronization created successfully! ID: {sync.Id}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}