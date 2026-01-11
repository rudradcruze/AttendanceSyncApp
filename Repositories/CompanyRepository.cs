using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;

namespace AttandanceSyncApp.Repositories
{
    /// <summary>
    /// Repository implementation for Company entity
    /// </summary>
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context) : base(context)
        {
        }

        public Company GetFirstCompany()
        {
            return _dbSet
                .OrderBy(c => c.Id)
                .FirstOrDefault();
        }

        public Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds)
        {
            return _dbSet
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .ToDictionary(c => c.Id, c => c.CompanyName);
        }
    }
}
