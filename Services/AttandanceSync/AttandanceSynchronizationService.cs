using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Services.AttandanceSync
{
    /// <summary>
    /// Service for managing attendance synchronization operations.
    /// Handles creation, retrieval, and status tracking of attendance synchronization records.
    /// </summary>
    public class AttandanceSynchronizationService : IAttandanceSynchronizationService
    {
        /// Unit of work for database operations.
        private readonly IUnitOfWork _unitOfWork;
        /// Company service for retrieving company information.
        private readonly ICompanyService _companyService;

        /// <summary>
        /// Initializes a new AttandanceSynchronizationService with required dependencies.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="companyService">The company service.</param>
        public AttandanceSynchronizationService(IUnitOfWork unitOfWork, ICompanyService companyService)
        {
            _unitOfWork = unitOfWork;
            _companyService = companyService;
        }

        /// <summary>
        /// Retrieves attendance synchronization records with pagination support.
        /// Includes company name resolution for each record.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of synchronization records.</returns>
        public ServiceResult<PagedResultDto<AttandanceSynchronizationDto>> GetSynchronizationsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalRecords = _unitOfWork.AttandanceSynchronizations.Count();

                // Get attendance records with pagination
                var attendanceRecords = _unitOfWork.AttandanceSynchronizations.GetPaged(page, pageSize);

                // Get distinct company IDs from records
                var companyIds = attendanceRecords.Select(a => a.CompanyId).Distinct().ToList();

                // Fetch company names using CompanyService
                var companies = _companyService.GetCompanyNamesByIds(companyIds);

                // Map to DTOs with company names
                var data = attendanceRecords.Select(a => new AttandanceSynchronizationDto
                {
                    Id = a.Id,
                    FromDate = a.FromDate,
                    ToDate = a.ToDate,
                    CompanyName = companies.ContainsKey(a.CompanyId) ? companies[a.CompanyId] : "N/A",
                    Status = a.Status
                }).ToList();

                var result = new PagedResultDto<AttandanceSynchronizationDto>
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    Data = data
                };

                return ServiceResult<PagedResultDto<AttandanceSynchronizationDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<AttandanceSynchronizationDto>>.FailureResult(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new attendance synchronization record.
        /// Validates date formats and business rules (ToDate must equal FromDate).
        /// </summary>
        /// <param name="fromDate">The start date in string format.</param>
        /// <param name="toDate">The end date in string format.</param>
        /// <returns>The ID of the created synchronization record.</returns>
        public ServiceResult<int> CreateSynchronization(string fromDate, string toDate)
        {
            try
            {
                // Validate FromDate
                if (!DateTime.TryParse(fromDate, out DateTime parsedFromDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid From Date format");
                }

                // Validate ToDate
                if (!DateTime.TryParse(toDate, out DateTime parsedToDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid To Date format");
                }

                // Business rule: ToDate must be equal to FromDate
                if (parsedToDate != parsedFromDate)
                {
                    return ServiceResult<int>.FailureResult("To Date must be the same as From Date");
                }

                // Get the first company for synchronization
                var firstCompany = _companyService.GetFirstCompany();
                if (firstCompany == null)
                {
                    return ServiceResult<int>.FailureResult("No company found in database.");
                }

                // Create new synchronization record
                var sync = new AttandanceSynchronization
                {
                    FromDate = parsedFromDate,
                    ToDate = parsedToDate,
                    CompanyId = firstCompany.Id,
                    Status = "NR" // New Request
                };

                _unitOfWork.AttandanceSynchronizations.Add(sync);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(sync.Id, $"Synchronization created successfully! ID: {sync.Id}");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the status of multiple synchronization records by their IDs.
        /// </summary>
        /// <param name="ids">Array of synchronization record IDs.</param>
        /// <returns>List of ID and status pairs.</returns>
        public ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids)
        {
            try
            {
                // Return empty list if no IDs provided
                if (ids == null || !ids.Any())
                {
                    return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(new List<StatusDto>());
                }

                // Fetch synchronizations by IDs
                var synchronizations = _unitOfWork.AttandanceSynchronizations.GetByIds(ids);

                // Map to status DTOs
                var statuses = synchronizations.Select(a => new StatusDto
                {
                    Id = a.Id,
                    Status = a.Status
                }).ToList();

                return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(statuses);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<StatusDto>>.FailureResult(ex.Message);
            }
        }
    }
}
