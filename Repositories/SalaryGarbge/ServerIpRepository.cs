using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.SalaryGarbge
{
    /// <summary>
    /// Repository for ServerIp entity operations.
    /// Manages server IP address records for the salary garbage collection module,
    /// tracking database servers that require cleanup operations.
    /// </summary>
    public class ServerIpRepository : Repository<ServerIp>, IServerIpRepository
    {
        /// <summary>
        /// Initializes a new ServerIpRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public ServerIpRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves a server record by its IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address to search for.</param>
        /// <returns>Server IP record if found, otherwise null.</returns>
        public ServerIp GetByIpAddress(string ipAddress)
        {
            // Use AsNoTracking for read-only query performance
            return _dbSet.AsNoTracking()
                .FirstOrDefault(s => s.IpAddress == ipAddress);
        }

        /// <summary>
        /// Checks if an IP address already exists in the system.
        /// </summary>
        /// <param name="ipAddress">The IP address to check.</param>
        /// <param name="excludeId">Optional server ID to exclude from the check (for update scenarios).</param>
        /// <returns>True if IP address exists, false otherwise.</returns>
        public bool IpAddressExists(string ipAddress, int? excludeId = null)
        {
            // Exclude specific ID when checking for duplicates during updates
            if (excludeId.HasValue)
            {
                return _dbSet.Any(s => s.IpAddress == ipAddress && s.Id != excludeId.Value);
            }
            return _dbSet.Any(s => s.IpAddress == ipAddress);
        }
    }
}
