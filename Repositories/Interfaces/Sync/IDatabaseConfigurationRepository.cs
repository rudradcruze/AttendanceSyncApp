using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface IDatabaseConfigurationRepository : IRepository<DatabaseConfiguration>
    {
        DatabaseConfiguration GetByRequestId(int requestId);
        bool HasConfiguration(int requestId);
    }
}
