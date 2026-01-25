using System;
using System.Collections.Generic;
using AttandanceSyncApp.Models.BranchIssue;

namespace AttandanceSyncApp.Repositories.Interfaces.BranchIssue
{
    public interface IBranchIssueRepository
    {
        DateTime GetLastMonthDate(string connectionString);
        IEnumerable<ProblemBranch> GetProblemBranches(string connectionString, DateTime monthStartDate, string locationId);
        void InsertProblemBranches(string connectionString, string month, string prevMonth);
        void ReprocessBranch(string connectionString, string branchCode, string month, string prevMonth);
    }
}
