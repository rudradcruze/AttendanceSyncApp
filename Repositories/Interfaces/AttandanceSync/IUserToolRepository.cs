using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface IUserToolRepository : IRepository<UserTool>
    {
        IEnumerable<UserTool> GetActiveToolsByUserId(int userId);
        IEnumerable<UserTool> GetAllAssignments(int page, int pageSize);
        int GetTotalAssignmentsCount();
        UserTool GetActiveAssignment(int userId, int toolId);
        bool HasActiveAssignment(int userId, int toolId);
        IEnumerable<UserTool> GetAssignmentsByToolId(int toolId);
        UserTool GetWithDetails(int id);
    }
}
