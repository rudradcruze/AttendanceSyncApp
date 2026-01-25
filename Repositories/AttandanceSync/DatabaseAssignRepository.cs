using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Repositories.AttandanceSync
{
    public class DatabaseAssignRepository : Repository<DatabaseAssign>, IDatabaseAssignRepository
    {
        private readonly AuthDbContext _authContext;

        public DatabaseAssignRepository(AuthDbContext context) : base(context)
        {
            _authContext = context;
        }

        public IEnumerable<DatabaseAssign> GetAllWithDetails()
        {
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .OrderByDescending(da => da.Id)
                .ToList();
        }

        public IEnumerable<DatabaseAssign> GetPaged(int page, int pageSize)
        {
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .OrderByDescending(da => da.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public DatabaseAssign GetWithDetails(int id)
        {
            return _dbSet.AsNoTracking()
                .Include(da => da.CompanyRequest)
                .Include(da => da.CompanyRequest.User)
                .Include(da => da.CompanyRequest.Employee)
                .Include(da => da.CompanyRequest.Company)
                .Include(da => da.CompanyRequest.Tool)
                .Include(da => da.AssignedByUser)
                .Include(da => da.DatabaseConfiguration)
                .Include(da => da.DatabaseConfiguration.Company)
                .FirstOrDefault(da => da.Id == id);
        }

        public DatabaseAssign GetByCompanyRequestId(int companyRequestId)
        {
            return _dbSet.AsNoTracking()
                .Include(da => da.DatabaseConfiguration)
                .FirstOrDefault(da => da.CompanyRequestId == companyRequestId);
        }

        public int GetTotalCount()
        {
            return _dbSet.Count();
        }

        public bool HasAssignment(int companyRequestId)
        {
            return _dbSet.Any(da => da.CompanyRequestId == companyRequestId);
        }
    }
}
