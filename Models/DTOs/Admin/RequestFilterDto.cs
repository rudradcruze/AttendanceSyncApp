using System;

namespace AttandanceSyncApp.Models.DTOs.Admin
{
    public class RequestFilterDto
    {
        public string UserSearch { get; set; }
        public int? CompanyId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}