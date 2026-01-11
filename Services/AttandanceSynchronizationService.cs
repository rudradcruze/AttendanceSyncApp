using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces;

namespace AttandanceSyncApp.Services
{
    /// <summary>
    /// Service implementation for AttandanceSynchronization business logic
    /// </summary>
    public class AttandanceSynchronizationService : IAttandanceSynchronizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyService _companyService;

        public AttandanceSynchronizationService(IUnitOfWork unitOfWork, ICompanyService companyService)
        {
            _unitOfWork = unitOfWork;
            _companyService = companyService;
        }

        public ServiceResult<PagedResultDto<AttandanceSynchronizationDto>> GetSynchronizationsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count
                var totalRecords = _unitOfWork.AttandanceSynchronizations.Count();

                // Get attendance records with pagination
                var attendanceRecords = _unitOfWork.AttandanceSynchronizations.GetPaged(page, pageSize);

                // Get all company IDs from the attendance records
                var companyIds = attendanceRecords.Select(a => a.CompanyId).Distinct().ToList();

                // Fetch companies using CompanyService
                var companies = _companyService.GetCompanyNamesByIds(companyIds);

                // Map to DTOs
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

                // Business rule: ToDate must be greater than or equal to FromDate
                if (parsedToDate < parsedFromDate)
                {
                    return ServiceResult<int>.FailureResult("To Date must be greater than or equal to From Date");
                }

                // Get the first company using CompanyService
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

        public ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(new List<StatusDto>());
                }

                var synchronizations = _unitOfWork.AttandanceSynchronizations.GetByIds(ids);

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
