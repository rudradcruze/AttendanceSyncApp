using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using AttendanceSyncApp.Models;

namespace AttendanceSyncApp.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: Attendance
        public ActionResult Index()
        {
            return View();
        }

        // GET: Get all synchronizations (AJAX)
        [HttpGet]
        public JsonResult GetSynchronizations()
        {
            try
            {
                var data = db.AttandanceSynchronizations
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
                    .AsEnumerable()
                    .Select(a => new
                    {
                        Id = a.Id,
                        FromDate = a.FromDate.ToString("yyyy-MM-dd"),
                        ToDate = a.ToDate.ToString("yyyy-MM-dd"),
                        a.CompanyName,
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

        // POST: Create new synchronization (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateSynchronization(string fromDate, string toDate)
        {
            try
            {
                if (!DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                {
                    return Json(new { success = false, message = "Invalid From Date format" });
                }

                if (!DateTime.TryParse(toDate, out DateTime parsedToDate))
                {
                    return Json(new { success = false, message = "Invalid To Date format" });
                }

                if (!db.Database.Exists())
                {
                    return Json(new { success = false, message = "Database does not exist!" });
                }

                var firstCompany = db.Companies.FirstOrDefault();
                if (firstCompany == null)
                {
                    return Json(new { success = false, message = "No company found in database." });
                }

                var sync = new AttandanceSynchronization
                {
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate,
                    CompanyId = firstCompany.CompanyId,
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
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}