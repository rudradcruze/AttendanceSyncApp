using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface ISyncCompanyRepository : IRepository<SyncCompany>
    {
        IEnumerable<SyncCompany> GetActiveCompanies();
        Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds);
    }
}
