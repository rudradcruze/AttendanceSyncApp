using System.Collections.Generic;
using AttandanceSyncApp.Models;

namespace AttandanceSyncApp.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for AttandanceSynchronization entity
    /// </summary>
    public interface IAttandanceSynchronizationRepository : IRepository<AttandanceSynchronization>
    {
        IEnumerable<AttandanceSynchronization> GetPaged(int page, int pageSize);
        IEnumerable<AttandanceSynchronization> GetByIds(int[] ids);
    }
}
