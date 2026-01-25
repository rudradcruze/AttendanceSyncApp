using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
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

        public IEnumerable<AttandanceSyncRequest> GetFiltered(string userSearch, int? companyId, string status, System.DateTime? fromDate, System.DateTime? toDate, int page, int pageSize, out int totalCount)
        {
            var query = _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Include(r => r.DatabaseConfiguration)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userSearch))
            {
                query = query.Where(r => r.User.Name.Contains(userSearch) || r.User.Email.Contains(userSearch));
            }

            if (companyId.HasValue)
            {
                query = query.Where(r => r.CompanyId == companyId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                // Status mapping: Pending (null), Completed (true), Failed (false)
                switch (status.ToLower())
                {
                    case "pending":
                    case "nr":
                        query = query.Where(r => r.IsSuccessful == null);
                        break;
                    case "completed":
                    case "success":
                    case "cp":
                        query = query.Where(r => r.IsSuccessful == true);
                        break;
                    case "failed":
                        query = query.Where(r => r.IsSuccessful == false);
                        break;
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the end date fully
                var nextDay = toDate.Value.AddDays(1);
                query = query.Where(r => r.CreatedAt < nextDay);
            }

            totalCount = query.Count();

            return query.OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
