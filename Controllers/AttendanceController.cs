using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using AttendanceSyncApp.Models;

namespace AttendanceSyncApp.Controllers
{
    public class AttendanceController : Controller
    {
        private AppDbContext db = new AppDbContext();

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
                    .AsEnumerable() // SWITCH TO MEMORY HERE
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
        public JsonResult CreateSynchronization(string fromDate, string toDate)
        {
            try
            {
                // Parse dates
                DateTime parsedFromDate;
                DateTime parsedToDate;

                if (!DateTime.TryParse(fromDate, out parsedFromDate))
                {
                    return Json(new { success = false, message = "Invalid From Date format" });
                }

                if (!DateTime.TryParse(toDate, out parsedToDate))
                {
                    return Json(new { success = false, message = "Invalid To Date format" });
                }

                // Check if database exists
                if (!db.Database.Exists())
                {
                    return Json(new { success = false, message = "Database does not exist!" });
                }

                // Get first company
                var firstCompany = db.Companies.FirstOrDefault();

                if (firstCompany == null)
                {
                    return Json(new { success = false, message = "No company found in database. Please run: UPDATE-DATABASE in Package Manager Console" });
                }

                var sync = new AttandanceSynchronization
                {
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate,
                    CompanyId = firstCompany.CompanyId,
                    Status = "NR" // Hardcoded as New Request
                };

                db.AttandanceSynchronizations.Add(sync);
                int result = db.SaveChanges();

                if (result > 0)
                {
                    return Json(new { success = true, message = "Synchronization created successfully! ID: " + sync.Id });
                }
                else
                {
                    return Json(new { success = false, message = "SaveChanges returned 0. No records were saved." });
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                string errors = string.Join("; ", dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage));
                return Json(new { success = false, message = "Validation Error: " + errors });
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                return Json(new { success = false, message = "SQL Error: " + sqlEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "") });
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