using System.Collections.Generic;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Repositories.Interfaces.AttandanceSync
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        IEnumerable<Employee> GetActiveEmployees();
        Dictionary<int, string> GetEmployeeNamesByIds(List<int> employeeIds);
    }
}
