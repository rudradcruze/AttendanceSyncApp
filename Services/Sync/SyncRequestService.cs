using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;

namespace AttandanceSyncApp.Services.Sync
{
    public class SyncRequestService : ISyncRequestService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public SyncRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<SyncRequestDto>> GetUserRequestsPaged(int userId, int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.AttandanceSyncRequests.GetTotalCountByUserId(userId);
                var requests = _unitOfWork.AttandanceSyncRequests.GetPagedByUserId(userId, page, pageSize)
                    .Select(r => new SyncRequestDto
                    {
                        Id = r.Id,
                        UserName = r.User?.Name ?? "Unknown",
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolName = r.Tool?.Name ?? "Unknown",
                        Email = r.Email,
                        Status = r.Status,
                        FromDate = r.FromDate,
                        ToDate = r.ToDate,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<SyncRequestDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<SyncRequestDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<SyncRequestDto>>.FailureResult($"Error retrieving requests: {ex.Message}");
            }
        }

        public ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, string userEmail, int sessionId)
        {
            try
            {
                // Validate dates
                DateTime? fromDate = null;
                DateTime? toDate = null;

                if (!string.IsNullOrEmpty(dto.FromDate))
                {
                    if (!DateTime.TryParse(dto.FromDate, out DateTime parsedFromDate))
                    {
                        return ServiceResult<int>.FailureResult("Invalid From Date format");
                    }
                    fromDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(dto.ToDate))
                {
                    if (!DateTime.TryParse(dto.ToDate, out DateTime parsedToDate))
                    {
                        return ServiceResult<int>.FailureResult("Invalid To Date format");
                    }
                    toDate = parsedToDate;
                }

                // Validate company exists
                var company = _unitOfWork.SyncCompanies.GetById(dto.CompanyId);
                if (company == null)
                {
                    return ServiceResult<int>.FailureResult("Selected company not found");
                }

                if (company.Status != "Active")
                {
                    return ServiceResult<int>.FailureResult("Selected company is not active");
                }

                // Validate tool exists
                var tool = _unitOfWork.Tools.GetById(dto.ToolId);
                if (tool == null || !tool.IsActive)
                {
                    return ServiceResult<int>.FailureResult("Selected tool not found or inactive");
                }

                // Create sync request
                var request = new AttandanceSyncRequest
                {
                    UserId = dto.UserId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId,
                    Email = userEmail,
                    Status = "NR",
                    SessionId = sessionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.AttandanceSyncRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(request.Id, "Sync request created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Error creating request: {ex.Message}");
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

                var requests = _unitOfWork.AttandanceSyncRequests.Find(r => ids.Contains(r.Id))
                    .Select(r => new StatusDto
                    {
                        Id = r.Id,
                        Status = r.Status
                    })
                    .ToList();

                return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(requests);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<StatusDto>>.FailureResult($"Error retrieving statuses: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<UserDto>> GetAllUsers()
        {
            try
            {
                var users = _unitOfWork.Users.GetAll()
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .ToList();

                return ServiceResult<IEnumerable<UserDto>>.SuccessResult(users);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UserDto>>.FailureResult($"Error retrieving users: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies()
        {
            try
            {
                var companies = _unitOfWork.SyncCompanies.GetActiveCompanies();
                return ServiceResult<IEnumerable<SyncCompany>>.SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<SyncCompany>>.FailureResult($"Error retrieving companies: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<Tool>> GetActiveTools()
        {
            try
            {
                var tools = _unitOfWork.Tools.GetActiveTools();
                return ServiceResult<IEnumerable<Tool>>.SuccessResult(tools);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Tool>>.FailureResult($"Error retrieving tools: {ex.Message}");
            }
        }
    }
}
