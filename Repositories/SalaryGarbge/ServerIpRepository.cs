using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.SalaryGarbge
{
    public class ServerIpRepository : Repository<ServerIp>, IServerIpRepository
    {
        public ServerIpRepository(AuthDbContext context) : base(context)
        {
        }

        public ServerIp GetByIpAddress(string ipAddress)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefault(s => s.IpAddress == ipAddress);
        }

        public bool IpAddressExists(string ipAddress, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return _dbSet.Any(s => s.IpAddress == ipAddress && s.Id != excludeId.Value);
            }
            return _dbSet.Any(s => s.IpAddress == ipAddress);
        }
    }
}
