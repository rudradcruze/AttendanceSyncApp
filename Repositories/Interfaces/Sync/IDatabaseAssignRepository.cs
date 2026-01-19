using System.Collections.Generic;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface IDatabaseAssignRepository : IRepository<DatabaseAssign>
    {
        IEnumerable<DatabaseAssign> GetAllWithDetails();
        IEnumerable<DatabaseAssign> GetPaged(int page, int pageSize);
        DatabaseAssign GetWithDetails(int id);
        DatabaseAssign GetByCompanyRequestId(int companyRequestId);
        int GetTotalCount();
        bool HasAssignment(int companyRequestId);
    }
}
