using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface IToolRepository : IRepository<Tool>
    {
        IEnumerable<Tool> GetActiveTools();
        Dictionary<int, string> GetToolNamesByIds(List<int> toolIds);
    }
}
