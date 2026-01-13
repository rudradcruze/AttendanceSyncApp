namespace AttandanceSyncApp.Models.DTOs.Sync
{
    public class DatabaseConnectionDto
    {
        public string DatabaseIP { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
    }
}
