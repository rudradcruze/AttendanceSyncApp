using System.Collections.Generic;
using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Repositories.Interfaces.Auth
{
    public interface ILoginSessionRepository : IRepository<LoginSession>
    {
        LoginSession GetByToken(string sessionToken);
        IEnumerable<LoginSession> GetActiveSessionsByUserId(int userId);
        void DeactivateAllUserSessions(int userId);
    }
}
