using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AttandanceSyncApp.Models.BranchIssue;
using AttandanceSyncApp.Repositories.Interfaces.BranchIssue;

namespace AttandanceSyncApp.Repositories.BranchIssue
{
    /// <summary>
    /// Repository for branch issue operations using raw SQL queries.
    /// Manages problem branch detection and reprocessing for period-end operations,
    /// interacting directly with external company databases for salary processing.
    /// </summary>
    public class BranchIssueRepository : IBranchIssueRepository
    {
        /// <summary>
        /// Retrieves the most recent period start date from the period end details table.
        /// </summary>
        /// <param name="connectionString">Database connection string for the target company database.</param>
        /// <returns>The maximum PeriodFrom date, or current month minus one if no data exists.</returns>
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

        /// <summary>
        /// Retrieves branches that have processing issues for a specific period.
        /// </summary>
        /// <param name="connectionString">Database connection string for the target company database.</param>
        /// <param name="monthStartDate">The period start date to check for issues.</param>
        /// <param name="locationId">Optional location/branch ID filter (empty string for all branches).</param>
        /// <returns>Collection of problem branches with issue descriptions.</returns>
        public IEnumerable<ProblemBranch> GetProblemBranches(string connectionString, DateTime monthStartDate, string locationId)
        {
            var list = new List<ProblemBranch>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Query to find branches with period-end processing issues
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
                        // Map database results to ProblemBranch model
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

        /// <summary>
        /// Inserts problem branch records by executing stored procedure logic.
        /// </summary>
        /// <param name="connectionString">Database connection string for the target company database.</param>
        /// <param name="month">The current month to process.</param>
        /// <param name="prevMonth">The previous month for comparison.</param>
        /// <remarks>Calls SP_Insert_ProblemBranches_ByLogic stored procedure to identify and record problematic branches.</remarks>
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

        /// <summary>
        /// Reprocesses a single branch for a specific period to resolve period-end issues.
        /// </summary>
        /// <param name="connectionString">Database connection string for the target company database.</param>
        /// <param name="branchCode">The branch/location code to reprocess.</param>
        /// <param name="month">The current month to reprocess.</param>
        /// <param name="prevMonth">The previous month for calculation.</param>
        /// <remarks>Calls SP_Reprocess_SingleBranch stored procedure to re-run period-end calculations for the branch.</remarks>
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
