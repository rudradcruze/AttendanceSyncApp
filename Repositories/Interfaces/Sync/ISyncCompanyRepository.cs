using System.Collections.Generic;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface ISyncCompanyRepository : IRepository<SyncCompany>
    {
        IEnumerable<SyncCompany> GetActiveCompanies();
        Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds);
    }
}
