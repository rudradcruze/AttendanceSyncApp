using System.Collections.Generic;
using AttandanceSyncApp.Models.DTOs;

namespace AttandanceSyncApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for AttandanceSynchronization business logic
    /// </summary>
    public interface IAttandanceSynchronizationService
    {
        ServiceResult<PagedResultDto<AttandanceSynchronizationDto>> GetSynchronizationsPaged(int page, int pageSize);
        ServiceResult<int> CreateSynchronization(string fromDate, string toDate);
        ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids);
    }
}
