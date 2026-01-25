using System;

namespace AttandanceSyncApp.Models.DTOs.AttandanceSync
{
    /// <summary>
    /// Data Transfer Object for AttandanceSynchronization
    /// </summary>
    public class AttandanceSynchronizationDto
    {
        public int Id { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
    }
}
