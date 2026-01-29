using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing company records from an administrative perspective.
    /// Handles CRUD operations for companies and status management.
    /// </summary>
    public class CompanyManagementService : ICompanyManagementService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new CompanyManagementService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public CompanyManagementService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all companies with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of companies with their details.</returns>
        public ServiceResult<PagedResultDto<CompanyManagementDto>> GetCompaniesPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.SyncCompanies.Count();

                // Retrieve paginated companies and map to DTOs
                var companies = _unitOfWork.SyncCompanies.GetAll()
                    .OrderByDescending(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CompanyManagementDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<CompanyManagementDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = companies
                };

                return ServiceResult<PagedResultDto<CompanyManagementDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<CompanyManagementDto>>.FailureResult($"Failed to retrieve companies: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific company by its ID.
        /// </summary>
        /// <param name="id">The company ID.</param>
        /// <returns>Company details including name, email, and status.</returns>
        public ServiceResult<CompanyManagementDto> GetCompanyById(int id)
        {
            try
            {
                // Fetch company by ID
                var company = _unitOfWork.SyncCompanies.GetById(id);
                if (company == null)
                {
                    return ServiceResult<CompanyManagementDto>.FailureResult("Company not found");
                }

                var dto = new CompanyManagementDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Email = company.Email,
                    Status = company.Status,
                    CreatedAt = company.CreatedAt,
                    UpdatedAt = company.UpdatedAt
                };

                return ServiceResult<CompanyManagementDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyManagementDto>.FailureResult($"Failed to retrieve company: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new company record.
        /// </summary>
        /// <param name="dto">The company data to create.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult CreateCompany(CompanyCreateDto dto)
        {
            try
            {
                // Validate company name
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Company name is required");
                }

                // Create new company entity
                var company = new SyncCompany
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email?.Trim(),
                    Status = dto.Status ?? "Active",
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.SyncCompanies.Add(company);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Company created");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to create company: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing company's information.
        /// </summary>
        /// <param name="dto">The updated company data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateCompany(CompanyUpdateDto dto)
        {
            try
            {
                // Retrieve existing company
                var company = _unitOfWork.SyncCompanies.GetById(dto.Id);
                if (company == null)
                {
                    return ServiceResult.FailureResult("Company not found");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Company name is required");
                }

                // Update company properties
                company.Name = dto.Name.Trim();
                company.Email = dto.Email?.Trim();
                company.Status = dto.Status;
                company.UpdatedAt = DateTime.Now;

                _unitOfWork.SyncCompanies.Update(company);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Company updated");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update company: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a company record if it has no associated requests.
        /// </summary>
        /// <param name="id">The company ID to delete.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult DeleteCompany(int id)
        {
            try
            {
                // Retrieve company to delete
                var company = _unitOfWork.SyncCompanies.GetById(id);
                if (company == null)
                {
                    return ServiceResult.FailureResult("Company not found");
                }

                // Check if company has sync requests (prevent deletion)
                var hasRequests = _unitOfWork.AttandanceSyncRequests.Count(r => r.CompanyId == id) > 0;
                if (hasRequests)
                {
                    return ServiceResult.FailureResult("Cannot delete company with existing requests");
                }

                _unitOfWork.SyncCompanies.Remove(company);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Company deleted");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to delete company: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the status of a company between Active and Inactive.
        /// </summary>
        /// <param name="id">The company ID to toggle.</param>
        /// <returns>Success or failure result with new status.</returns>
        public ServiceResult ToggleCompanyStatus(int id)
        {
            try
            {
                // Retrieve the company
                var company = _unitOfWork.SyncCompanies.GetById(id);
                if (company == null)
                {
                    return ServiceResult.FailureResult("Company not found");
                }

                // Toggle status between Active and Inactive
                company.Status = company.Status == "Active" ? "Inactive" : "Active";
                company.UpdatedAt = DateTime.Now;

                _unitOfWork.SyncCompanies.Update(company);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult($"Company {company.Status.ToLower()}");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to toggle company status: {ex.Message}");
            }
        }
    }
}
