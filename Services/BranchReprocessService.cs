using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AttendanceSyncApp.Models;
namespace AttendanceSyncApp.Services
{
    public class BranchReprocessService
    {
        string conStr = ConfigurationManager.ConnectionStrings["DefaultConn"].ConnectionString;

        public string GetLastMonth()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(
                  "SELECT TOP 1 ProblemMonth FROM ProblemBranches ORDER BY CreatedAt DESC", con);
                con.Open();
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        public void InsertProblemBranches(string month, string prevMonth)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand("SP_Insert_ProblemBranches_ByLogic", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@PrevMonth", prevMonth);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<ProblemBranchVM> GetBranches(string month)
        {
            List<ProblemBranchVM> list = new List<ProblemBranchVM>();

            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(
                  "SELECT BranchCode, BranchName FROM ProblemBranches WHERE ProblemMonth=@m", con);
                cmd.Parameters.AddWithValue("@m", month);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ProblemBranchVM
                    {
                        BranchCode = dr["BranchCode"].ToString(),
                        BranchName = dr["BranchName"].ToString()
                    });
                }
            }
            return list;
        }

        public void Reprocess(string code, string month, string prevMonth)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand("SP_Reprocess_SingleBranch", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BranchCode", code);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@PrevMonth", prevMonth);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
