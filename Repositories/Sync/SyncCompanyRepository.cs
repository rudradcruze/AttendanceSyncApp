using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    public class SyncCompanyRepository : Repository<SyncCompany>, ISyncCompanyRepository
    {
        public SyncCompanyRepository(AuthDbContext context) : base(context)
        {
        }

        public IEnumerable<SyncCompany> GetActiveCompanies()
        {
            return _dbSet.AsNoTracking()
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds)
        {
            return _dbSet.AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.Name);
        }
    }
}
