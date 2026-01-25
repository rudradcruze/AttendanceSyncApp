using System.Collections.Generic;

namespace AttandanceSyncApp.Models.DTOs.BranchIssue
{
    public class ProblemBranchDto
    {
        public string PeriodFrom { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Remarks { get; set; }
    }

    public class BranchIssueRequestDto
    {
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
        public string MonthStartDate { get; set; }
        public string LocationId { get; set; }
    }

    public class ReprocessBranchRequestDto
    {
        public int ServerIpId { get; set; }
        public string DatabaseName { get; set; }
        public string BranchCode { get; set; }
        public string Month { get; set; }
        public string PrevMonth { get; set; }
    }
}
