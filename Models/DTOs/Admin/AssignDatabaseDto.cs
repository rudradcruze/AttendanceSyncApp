namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class AssignDatabaseDto
    {
        public int RequestId { get; set; }
        public string DatabaseIP { get; set; }
        public string DatabaseUserId { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
    }
}
