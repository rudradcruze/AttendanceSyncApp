using System.Collections.Generic;

namespace AttandanceSyncApp.Models.DTOs.SalaryGarbge
{
    /// <summary>
    /// DTO for employee garbage data (employees with GradeScaleId or BasicSalary issues)
    /// </summary>
    public class GarbageDataDto
    {
        public string ServerIp { get; set; }
        public string DatabaseName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Problem { get; set; }
        public string EmployeeCode { get; set; }
    }

    /// <summary>
    /// DTO for problematic garbage data (salary mismatches between tables)
    /// </summary>
    public class ProblematicGarbageDto
    {
        public string ServerIp { get; set; }
        public string DatabaseName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string IssueTableName { get; set; }
        public decimal CurrentBasicSalary { get; set; }
        public decimal ExpectedBasicSalary { get; set; }
        public string EmployeeCode { get; set; }
    }

    /// <summary>
    /// DTO for problematic garbage scan result
    /// </summary>
    public class ProblematicScanResultDto
    {
        public List<ProblematicGarbageDto> ProblematicData { get; set; }
        public int TotalServers { get; set; }
        public int TotalDatabases { get; set; }
        public int TotalProblematicRecords { get; set; }
        public string Summary { get; set; }

        public ProblematicScanResultDto()
        {
            ProblematicData = new List<ProblematicGarbageDto>();
        }
    }

    /// <summary>
    /// DTO for scan progress updates
    /// </summary>
    public class ScanProgressDto
    {
        public string ServerIp { get; set; }
        public string DatabaseName { get; set; }
        public string Status { get; set; } // "scanning", "completed", "error"
        public string Message { get; set; }
        public int TotalDatabases { get; set; }
        public int CurrentDatabase { get; set; }
    }

    /// <summary>
    /// DTO for the complete scan result
    /// </summary>
    public class GarbageScanResultDto
    {
        public List<GarbageDataDto> GarbageData { get; set; }
        public int TotalServers { get; set; }
        public int TotalDatabases { get; set; }
        public int TotalGarbageRecords { get; set; }
        public string Summary { get; set; }

        public GarbageScanResultDto()
        {
            GarbageData = new List<GarbageDataDto>();
        }
    }
}
