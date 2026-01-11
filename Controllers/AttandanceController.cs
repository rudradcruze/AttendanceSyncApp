using System;
using System.Collections.Generic;
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
                // Get total count from base table
                var totalRecords = db.AttandanceSynchronizations.Count();

                // Get attendance records with pagination
                var attendanceRecords = db.AttandanceSynchronizations
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Get all company IDs from the attendance records
                var companyIds = attendanceRecords.Select(a => a.CompanyId).Distinct().ToList();

                // Fetch companies in one query and create a dictionary
                var companies = db.Companies
                    .AsNoTracking()
                    .Where(c => companyIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.CompanyName);

                // Join in memory - this will ALWAYS show all attendance records
                var data = attendanceRecords.Select(a => new
                {
                    a.Id,
                    a.FromDate,
                    a.ToDate,
                    CompanyName = companies.ContainsKey(a.CompanyId) ? companies[a.CompanyId] : "N/A",
                    a.Status
                }).ToList();

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
                // Get all attendance records
                var attendanceRecords = db.AttandanceSynchronizations
                    .AsNoTracking()
                    .OrderByDescending(a => a.Id)
                    .ToList();

                // Get all company IDs
                var companyIds = attendanceRecords.Select(a => a.CompanyId).Distinct().ToList();

                // Fetch companies
                var companies = db.Companies
                    .AsNoTracking()
                    .Where(c => companyIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.CompanyName);

                // Join in memory
                var data = attendanceRecords.Select(a => new
                {
                    a.Id,
                    a.FromDate,
                    a.ToDate,
                    CompanyName = companies.ContainsKey(a.CompanyId) ? companies[a.CompanyId] : "N/A",
                    a.Status
                }).ToList();

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
                var firstCompany = db.Companies
                    .OrderBy(c => c.Id)
                    .FirstOrDefault();
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

        [HttpPost]
        public JsonResult GetStatusesByIds(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);

                var statuses = db.AttandanceSynchronizations
                    .AsNoTracking()
                    .Where(a => ids.Contains(a.Id))
                    .Select(a => new { a.Id, a.Status })
                    .ToList();

                return Json(statuses, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
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