using System.Collections.Generic;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces;

namespace AttandanceSyncApp.Services
{
    /// <summary>
    /// Service for managing company operations.
    /// Provides methods for retrieving company information.
    /// </summary>
    public class CompanyService : ICompanyService
    {
        /// Unit of work for database operations.
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new CompanyService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves the first company in the database.
        /// </summary>
        /// <returns>The first company record.</returns>
        public Company GetFirstCompany()
        {
            // Get the first company from repository
            return _unitOfWork.Companies.GetFirstCompany();
        }

        /// <summary>
        /// Retrieves company names for a collection of company IDs.
        /// </summary>
        /// <param name="companyIds">List of company IDs.</param>
        /// <returns>Dictionary mapping company ID to company name.</returns>
        public Dictionary<int, string> GetCompanyNamesByIds(List<int> companyIds)
        {
            // Get company names by IDs
            return _unitOfWork.Companies.GetCompanyNamesByIds(companyIds);
        }
    }
}
