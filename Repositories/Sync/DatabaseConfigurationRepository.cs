using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    public class DatabaseConfigurationRepository : Repository<DatabaseConfiguration>, IDatabaseConfigurationRepository
    {
        public DatabaseConfigurationRepository(AuthDbContext context) : base(context)
        {
        }

        public DatabaseConfiguration GetByCompanyId(int companyId)
        {
            return _dbSet.AsNoTracking()
                //.Include(dc => dc.Company) // SyncCompany
                .FirstOrDefault(dc => dc.CompanyId == companyId);
        }

        public bool HasConfiguration(int companyId)
        {
            return _dbSet.Any(dc => dc.CompanyId == companyId);
        }
    }
}
