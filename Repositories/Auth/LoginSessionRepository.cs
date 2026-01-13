using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Repositories.Interfaces.Auth;

namespace AttandanceSyncApp.Repositories.Auth
{
    public class LoginSessionRepository : Repository<LoginSession>, ILoginSessionRepository
    {
        private readonly AuthDbContext _authContext;

        public LoginSessionRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        public LoginSession GetByToken(string sessionToken)
        {
            return _dbSet.AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefault(s => s.SessionToken == sessionToken && s.IsActive);
        }

        public IEnumerable<LoginSession> GetActiveSessionsByUserId(int userId)
        {
            return _dbSet.AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LoginTime)
                .ToList();
        }

        public void DeactivateAllUserSessions(int userId)
        {
            var sessions = _authContext.LoginSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToList();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.LogoutTime = System.DateTime.Now;
                session.UpdatedAt = System.DateTime.Now;
            }
        }
    }
}
