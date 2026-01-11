using System.Collections.Generic;
using AttandanceSyncApp.Models;

namespace AttandanceSyncApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for Company business logic
    /// </summary>
    public interface ICompanyService
    {
        Company GetFirstCompany();
        Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds);
    }
}
