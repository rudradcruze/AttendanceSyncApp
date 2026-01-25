using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface IAttandanceSyncRequestRepository : IRepository<AttandanceSyncRequest>
    {
        IEnumerable<AttandanceSyncRequest> GetByUserId(int userId);
        IEnumerable<AttandanceSyncRequest> GetAllWithDetails();
        IEnumerable<AttandanceSyncRequest> GetPaged(int page, int pageSize);
        IEnumerable<AttandanceSyncRequest> GetPagedByUserId(int userId, int page, int pageSize);
        AttandanceSyncRequest GetWithConfiguration(int id);
        int GetTotalCount();
        int GetTotalCountByUserId(int userId);
        IEnumerable<AttandanceSyncRequest> GetFiltered(string userSearch, int? companyId, string status, System.DateTime? fromDate, System.DateTime? toDate, int page, int pageSize, out int totalCount);
    }
}
