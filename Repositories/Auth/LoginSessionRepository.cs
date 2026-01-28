using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Repositories.Interfaces.Auth;

namespace AttandanceSyncApp.Repositories.Auth
{
    /// <summary>
    /// Repository for LoginSession entity operations.
    /// Manages user authentication sessions including token validation,
    /// session tracking, and multi-device session management.
    /// </summary>
    public class LoginSessionRepository : Repository<LoginSession>, ILoginSessionRepository
    {
        /// Reference to the authentication context for session management.
        private readonly AuthDbContext _authContext;

        /// <summary>
        /// Initializes a new LoginSessionRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public LoginSessionRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        /// <summary>
        /// Retrieves an active session by its unique token.
        /// </summary>
        /// <param name="sessionToken">The session token to search for.</param>
        /// <returns>Active session with associated user data, or null if not found or inactive.</returns>
        public LoginSession GetByToken(string sessionToken)
        {
            // Include user data and filter for active sessions only
            return _dbSet.AsNoTracking()
                .Include(s => s.User)
                .FirstOrDefault(s => s.SessionToken == sessionToken && s.IsActive);
        }

        /// <summary>
        /// Retrieves all active sessions for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to search sessions for.</param>
        /// <returns>Collection of active sessions ordered by login time (newest first).</returns>
        public IEnumerable<LoginSession> GetActiveSessionsByUserId(int userId)
        {
            // Return active sessions ordered by most recent login
            return _dbSet.AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LoginTime)
                .ToList();
        }

        /// <summary>
        /// Deactivates all active sessions for a specific user.
        /// Used for logout-all-devices functionality or security measures.
        /// </summary>
        /// <param name="userId">The user ID whose sessions should be deactivated.</param>
        /// <remarks>Changes are tracked but not saved; call SaveChanges on unit of work to persist.</remarks>
        public void DeactivateAllUserSessions(int userId)
        {
            // Find all active sessions for the user
            var sessions = _authContext.LoginSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToList();

            // Deactivate each session and record the logout time
            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.LogoutTime = System.DateTime.Now;
                session.UpdatedAt = System.DateTime.Now;
            }
        }
    }
}
