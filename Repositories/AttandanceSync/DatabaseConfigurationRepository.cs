using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
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
