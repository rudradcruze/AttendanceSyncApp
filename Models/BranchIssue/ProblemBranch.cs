using System;

namespace AttandanceSyncApp.Models.BranchIssue
{
    public class ProblemBranch
    {
        public string PeriodFrom { get; set; }
        public string BranchCode { get; set; }   // LocationID
        public string BranchName { get; set; }   // LocationName
        public string Remarks { get; set; }
    }
}
