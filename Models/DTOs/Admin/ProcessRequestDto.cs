namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class ProcessRequestDto
    {
        public int RequestId { get; set; }
        public int? ExternalSyncId { get; set; }
        public bool IsSuccessful { get; set; }
    }
}