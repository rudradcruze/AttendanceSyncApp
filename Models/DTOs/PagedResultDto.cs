using System.Collections.Generic;

namespace AttandanceSyncApp.Models.DTOs
{
    /// <summary>
    /// Generic Data Transfer Object for paginated results
    /// </summary>
    public class PagedResultDto<T>
    {
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
