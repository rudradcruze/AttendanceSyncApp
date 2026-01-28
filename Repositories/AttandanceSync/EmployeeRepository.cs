using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    /// <summary>
    /// Repository for Employee entity operations.
    /// Manages employee records for attendance synchronization,
    /// providing filtering and batch retrieval capabilities.
    /// </summary>
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        /// <summary>
        /// Initializes a new EmployeeRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public EmployeeRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves all active employees.
        /// </summary>
        /// <returns>Collection of active employees ordered alphabetically by name.</returns>
        public IEnumerable<Employee> GetActiveEmployees()
        {
            // Filter by IsActive flag and order by name
            return _dbSet.AsNoTracking()
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves employee names for a collection of employee IDs.
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to retrieve names for.</param>
        /// <returns>Dictionary mapping employee ID to employee name.</returns>
        public Dictionary<int, string> GetEmployeeNamesByIds(List<int> employeeIds)
        {
            // Use AsNoTracking for read-only performance
            // Return dictionary for efficient lookup
            return _dbSet.AsNoTracking()
                .Where(e => employeeIds.Contains(e.Id))
                .ToDictionary(e => e.Id, e => e.Name);
        }
    }
}
