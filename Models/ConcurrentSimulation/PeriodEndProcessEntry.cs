namespace AttandanceSyncApp.Models.ConcurrentSimulation
{
    /// <summary>
    /// Represents a single entry for PeriodEndProcessRequest
    /// Used for both query results and insert operations
    /// </summary>
    public class PeriodEndProcessEntry
    {
        public string UserId { get; set; }
        public int Branch_Id { get; set; }
        public int Location_Id { get; set; }
        public int CompanyId { get; set; }
        public string Status { get; set; }
        public int EmployeeId { get; set; }
        public string PostProcessStatus { get; set; }
    }
}
