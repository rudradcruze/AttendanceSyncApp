using System.Collections.Generic;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces;

namespace AttandanceSyncApp.Services
{
    /// <summary>
    /// Service implementation for Company business logic
    /// </summary>
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Company GetFirstCompany()
        {
            return _unitOfWork.Companies.GetFirstCompany();
        }

        public Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds)
        {
            return _unitOfWork.Companies.GetCompanyNamesByIds(companyIds);
        }
    }
}
