using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    public class ToolRepository : Repository<Tool>, IToolRepository
    {
        public ToolRepository(AuthDbContext context) : base(context)
        {
        }

        public IEnumerable<Tool> GetActiveTools()
        {
            return _dbSet.AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToList();
        }

        public Dictionary<int, string> GetToolNamesByIds(List<int> toolIds)
        {
            return _dbSet.AsNoTracking()
                .Where(t => toolIds.Contains(t.Id))
                .ToDictionary(t => t.Id, t => t.Name);
        }
    }
}
