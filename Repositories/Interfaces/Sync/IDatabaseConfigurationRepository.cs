using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface IDatabaseConfigurationRepository : IRepository<DatabaseConfiguration>
    {
        DatabaseConfiguration GetByCompanyId(int companyId);
        bool HasConfiguration(int companyId);
    }
}