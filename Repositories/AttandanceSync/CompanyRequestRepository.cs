using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    public class CompanyRequestRepository : Repository<CompanyRequest>, ICompanyRequestRepository
    {
        private readonly AuthDbContext _authContext;

        public CompanyRequestRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        public IEnumerable<CompanyRequest> GetByUserId(int userId)
        {
            return _dbSet.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        public IEnumerable<CompanyRequest> GetAllWithDetails()
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .OrderByDescending(r => r.Id)
                .ToList();
        }

        public IEnumerable<CompanyRequest> GetPaged(int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public IEnumerable<CompanyRequest> GetPagedByUserId(int userId, int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public CompanyRequest GetWithDetails(int id)
        {
            return _dbSet.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.Tool)
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
