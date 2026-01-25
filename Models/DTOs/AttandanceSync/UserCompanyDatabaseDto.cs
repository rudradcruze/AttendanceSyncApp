namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    public class UserCompanyDatabaseDto
    {
        public int CompanyRequestId { get; set; }
        public int DatabaseAssignmentId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string DatabaseName { get; set; }
        public int DatabaseConfigurationId { get; set; }
        public int ToolId { get; set; }
        public string ToolName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
    }
}
