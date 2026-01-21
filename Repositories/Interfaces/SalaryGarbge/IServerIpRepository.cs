using AttandanceSyncApp.Models.SalaryGarbge;

namespace AttandanceSyncApp.Repositories.Interfaces.SalaryGarbge
{
    public interface IServerIpRepository : IRepository<ServerIp>
    {
        ServerIp GetByIpAddress(string ipAddress);
        bool IpAddressExists(string ipAddress, int? excludeId = null);
    }
}
