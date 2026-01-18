using System.Collections.Generic;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Repositories.Interfaces.Sync
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        IEnumerable<Employee> GetActiveEmployees();
        Dictionary<int, string> GetEmployeeNamesByIds(List<int> employeeIds);
    }
}
