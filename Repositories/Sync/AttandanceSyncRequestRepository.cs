using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces.Sync;

namespace AttandanceSyncApp.Repositories.Sync
{
    public class AttandanceSyncRequestRepository : Repository<AttandanceSyncRequest>, IAttandanceSyncRequestRepository
    {
        private readonly AuthDbContext _authContext;

        public AttandanceSyncRequestRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        public IEnumerable<AttandanceSyncRequest> GetByUserId(int userId)
        {
            return _dbSet.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        public IEnumerable<AttandanceSyncRequest> GetAllWithDetails()
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        public IEnumerable<AttandanceSyncRequest> GetPaged(int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public IEnumerable<AttandanceSyncRequest> GetPagedByUserId(int userId, int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public AttandanceSyncRequest GetWithConfiguration(int id)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .FirstOrDefault(r => r.Id == id);
        }

        public int GetTotalCount()
        {
            return _dbSet.Count();
        }

        public int GetTotalCountByUserId(int userId)
        {
            return _dbSet.Count(r => r.UserId == userId);
        }
    }
}
