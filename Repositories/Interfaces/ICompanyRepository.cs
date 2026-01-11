using System.Collections.Generic;
using AttandanceSyncApp.Models;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Company entity
    /// </summary>
    public interface ICompanyRepository : IRepository<Company>
    {
        Company GetFirstCompany();
        Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds);
    }
}
