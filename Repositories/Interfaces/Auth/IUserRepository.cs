using AttandanceSyncApp.Models.Auth;

namespace AttandanceSyncApp.Repositories.Interfaces.Auth
{
    public interface IUserRepository : IRepository<User>
    {
        User GetByEmail(string email);
        User GetByGoogleId(string googleId);
        bool EmailExists(string email);
    }
}
