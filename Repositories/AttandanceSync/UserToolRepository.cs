using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    public class UserToolRepository : Repository<UserTool>, IUserToolRepository
    {
        public UserToolRepository(AuthDbContext context) : base(context)
        {
        }

        public IEnumerable<UserTool> GetActiveToolsByUserId(int userId)
        {
            return _dbSet.AsNoTracking()
                .Include(ut => ut.Tool)
                .Include(ut => ut.User)
                .Include(ut => ut.AssignedByUser)
                .Where(ut => ut.UserId == userId && !ut.IsRevoked && ut.Tool.IsActive)
                .OrderBy(ut => ut.Tool.Name)
                .ToList();
        }

        public IEnumerable<UserTool> GetAllAssignments(int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Include(ut => ut.Tool)
                .Include(ut => ut.AssignedByUser)
                .OrderByDescending(ut => ut.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalAssignmentsCount()
        {
            return _dbSet.Count();
        }

        public UserTool GetActiveAssignment(int userId, int toolId)
        {
            return _dbSet.FirstOrDefault(ut =>
                ut.UserId == userId &&
                ut.ToolId == toolId &&
                !ut.IsRevoked);
        }

        public bool HasActiveAssignment(int userId, int toolId)
        {
            return _dbSet.Any(ut =>
                ut.UserId == userId &&
                ut.ToolId == toolId &&
                !ut.IsRevoked);
        }

        public IEnumerable<UserTool> GetAssignmentsByToolId(int toolId)
        {
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Where(ut => ut.ToolId == toolId && !ut.IsRevoked)
                .OrderBy(ut => ut.User.Name)
                .ToList();
        }

        public UserTool GetWithDetails(int id)
        {
            return _dbSet.AsNoTracking()
                .Include(ut => ut.User)
                .Include(ut => ut.Tool)
                .Include(ut => ut.AssignedByUser)
                .FirstOrDefault(ut => ut.Id == id);
        }
    }
}
