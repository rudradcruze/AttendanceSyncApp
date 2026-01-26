using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AttandanceSyncApp.Models.BranchIssue;
using AttandanceSyncApp.Repositories.Interfaces.BranchIssue;

namespace AttandanceSyncApp.Repositories.BranchIssue
{
    public class BranchIssueRepository : IBranchIssueRepository
    {
        public DateTime GetLastMonthDate(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string q = @"SELECT MAX(PeriodFrom) FROM far.tblperiodenddetails";
                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDateTime(result);
                }
                // Fallback or default if null
                return DateTime.Now.AddMonths(-1);
            }
        }

        public IEnumerable<ProblemBranch> GetProblemBranches(string connectionString, DateTime monthStartDate, string locationId)
        {
            var list = new List<ProblemBranch>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        p.PeriodFrom,
                        p.Location_Id AS LocationID,
                        ISNULL(l.LocationName, '') AS LocationName,
                        CASE
                            WHEN p.IsVoucherSend IS NULL OR p.IsVoucherSend = 0 THEN 'Period not processed - Voucher not sent'
                            WHEN p.IsTransactionDoneInCurrentPeriod IS NULL OR p.IsTransactionDoneInCurrentPeriod = 0 THEN 'No transactions in current period'
                            ELSE 'Period requires attention'
                        END AS Remarks
                    FROM far.tblperiodenddetails p
                    INNER JOIN far.tblLocation l ON l.Id = p.Location_Id
                    WHERE p.PeriodFrom = @monthStartDate
                        AND (@locationId = '' OR p.Location_Id = @locationId)
                        AND (p.IsVoucherSend IS NULL OR p.IsVoucherSend = 0 OR p.IsTransactionDoneInCurrentPeriod IS NULL OR p.IsTransactionDoneInCurrentPeriod = 0)
                    ORDER BY p.Location_Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@monthStartDate", monthStartDate);
                    cmd.Parameters.AddWithValue("@locationId", string.IsNullOrWhiteSpace(locationId) ? "" : locationId);

                    con.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new ProblemBranch
                            {
                                PeriodFrom = Convert.ToDateTime(rdr["PeriodFrom"]).ToString("MMM yyyy"),
                                BranchCode = rdr["LocationID"].ToString(),
                                BranchName = rdr["LocationName"].ToString(),
                                Remarks = rdr["Remarks"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void InsertProblemBranches(string connectionString, string month, string prevMonth)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Insert_ProblemBranches_ByLogic", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@PrevMonth", prevMonth);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ReprocessBranch(string connectionString, string branchCode, string month, string prevMonth)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_Reprocess_SingleBranch", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(" @BranchCode", branchCode);
                cmd.Parameters.AddWithValue(" @Month", month);
                cmd.Parameters.AddWithValue(" @PrevMonth", prevMonth);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
