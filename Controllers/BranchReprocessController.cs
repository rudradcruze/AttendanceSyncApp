using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using AttendanceSyncApp.Models;

namespace AttendanceSyncApp.Controllers
{
    public class BranchReprocessController : Controller
    {
        string conStr = ConfigurationManager
                        .ConnectionStrings["DefaultConnection"]
                        .ConnectionString;

        // ========= INDEX =========
        public ActionResult Index()
        {
            ViewBag.LastMonth = GetLastMonthDate();
            return View();
        }

        // ===== LOAD PROBLEM BRANCHES =====
        [HttpPost]
        public ActionResult LoadProblemBranches(string monthStartDate, string locationId)
        {
            try
            {
                List<ProblemBranchVM> list = new List<ProblemBranchVM>();

                using (SqlConnection con = new SqlConnection(conStr))
                using (SqlCommand cmd = new SqlCommand("dbo.sp_GetPeriodEndStatus", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@monthStartDate", DateTime.Parse(monthStartDate));
                    cmd.Parameters.AddWithValue("@locationId",
                        string.IsNullOrWhiteSpace(locationId) ? "" : locationId);

                    con.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new ProblemBranchVM
                            {
                                PeriodFrom = Convert.ToDateTime(rdr["PeriodFrom"]).ToString("MMM yyyy"),
                                BranchCode = rdr["LocationID"].ToString(),
                                BranchName = rdr["LocationName"].ToString(),
                                Remarks = rdr["Remarks"].ToString()
                            });
                        }
                    }
                }

                return PartialView("_ProblemBranchTable", list);
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.Message);
            }
        }

        // ===== LAST MONTH DATE =====
        private DateTime GetLastMonthDate()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                string q = @"SELECT MAX(PeriodFrom) FROM far.tblperiodenddetails";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                return Convert.ToDateTime(cmd.ExecuteScalar());
            }
        }
    }
}
