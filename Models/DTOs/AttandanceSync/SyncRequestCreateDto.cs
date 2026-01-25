namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    public class SyncRequestCreateDto
    {
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        public int ToolId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
