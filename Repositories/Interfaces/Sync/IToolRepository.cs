using System.Collections.Generic;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface IToolRepository : IRepository<Tool>
    {
        IEnumerable<Tool> GetActiveTools();
        Dictionary<int, string> GetToolNamesByIds(List<int> toolIds);
    }
}
