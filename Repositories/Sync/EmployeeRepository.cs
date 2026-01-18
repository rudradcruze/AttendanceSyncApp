using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AuthDbContext context) : base(context)
        {
        }

        public IEnumerable<Employee> GetActiveEmployees()
        {
            return _dbSet.AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToList();
        }

        public Dictionary<int, string> GetEmployeeNamesByIds(List<int> employeeIds)
        {
            return _dbSet.AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionary(e => e.Id, e => e.Name);
        }
    }
}
