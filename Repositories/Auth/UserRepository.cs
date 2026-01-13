using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Repositories.Interfaces.Auth;

namespace AttandanceSyncApp.Repositories.Auth
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AuthDbContext context) : base(context)
        {
        }

        public User GetByEmail(string email)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
        }

        public User GetByGoogleId(string googleId)
        {
            return _dbSet.AsNoTracking()
                .FirstOrDefault(u => u.GoogleId == googleId);
        }

        public bool EmailExists(string email)
        {
            return _dbSet.Any(u => u.Email == email);
        }
    }
}
