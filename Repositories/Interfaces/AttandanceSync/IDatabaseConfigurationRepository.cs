using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface IDatabaseConfigurationRepository : IRepository<DatabaseConfiguration>
    {
        DatabaseConfiguration GetByCompanyId(int companyId);
        bool HasConfiguration(int companyId);
    }
}
