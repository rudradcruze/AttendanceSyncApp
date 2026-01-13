namespace AttandanceSyncApp.Models.DTOs.Sync
{
    public class SyncRequestCreateDto
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int ToolId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
