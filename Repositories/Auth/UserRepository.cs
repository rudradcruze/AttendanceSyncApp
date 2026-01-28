using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Repositories.Interfaces.Auth;

namespace AttandanceSyncApp.Repositories.Auth
{
    /// <summary>
    /// Repository for User entity operations.
    /// Provides specialized methods for user lookup and authentication-related queries.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Initializes a new UserRepository with the given authentication context.
        /// </summary>
        /// <param name="context">The authentication database context.</param>
        public UserRepository(AuthDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>User with the specified email, or null if not found.</returns>
        public User GetByEmail(string email)
        {
            // Use AsNoTracking for read-only query performance
            return _dbSet.AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
        }

        /// <summary>
        /// Retrieves a user by their Google OAuth ID.
        /// </summary>
        /// <param name="googleId">The Google ID to search for.</param>
        /// <returns>User with the specified Google ID, or null if not found.</returns>
        public User GetByGoogleId(string googleId)
        {
            // Use AsNoTracking for read-only query performance
            return _dbSet.AsNoTracking()
                .FirstOrDefault(u => u.GoogleId == googleId);
        }

        /// <summary>
        /// Checks if an email address is already registered in the system.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if email exists, false otherwise.</returns>
        public bool EmailExists(string email)
        {
            return _dbSet.Any(u => u.Email == email);
        }
    }
}
