using System.Collections.Generic;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface ICompanyRequestRepository : IRepository<CompanyRequest>
    {
        IEnumerable<CompanyRequest> GetByUserId(int userId);
        IEnumerable<CompanyRequest> GetAllWithDetails();
        IEnumerable<CompanyRequest> GetPaged(int page, int pageSize);
        IEnumerable<CompanyRequest> GetPagedByUserId(int userId, int page, int pageSize);
        CompanyRequest GetWithDetails(int id);
        int GetTotalCount();
        int GetTotalCountByUserId(int userId);
    }
}
