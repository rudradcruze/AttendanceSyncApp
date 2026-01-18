using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
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
                        EmployeeName = r.Employee?.Name ?? "Unknown",
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolName = r.Tool?.Name ?? "Unknown",
                        ExternalSyncId = r.ExternalSyncId,
                        IsSuccessful = r.IsSuccessful,
                        Status = GetStatusText(r.IsSuccessful),
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
                return ServiceResult<PagedResultDto<SyncRequestDto>>.FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        public ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate dates
                if (!DateTime.TryParse(dto.FromDate, out DateTime fromDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid From Date format");
                }

                if (!DateTime.TryParse(dto.ToDate, out DateTime toDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid To Date format");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<int>.FailureResult("From Date cannot be after To Date");
                }

                // Validate employee exists
                var employee = _unitOfWork.Employees.GetById(dto.EmployeeId);
                if (employee == null || !employee.IsActive)
                {
                    return ServiceResult<int>.FailureResult("Selected employee not found or inactive");
                }

                // Validate company exists
                var company = _unitOfWork.SyncCompanies.GetById(dto.CompanyId);
                if (company == null || company.Status != "Active")
                {
                    return ServiceResult<int>.FailureResult("Selected company not found or inactive");
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
                    UserId = userId,
                    EmployeeId = dto.EmployeeId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId,
                    SessionId = sessionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    IsSuccessful = null, // Pending
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.AttandanceSyncRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(request.Id, "Request created");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to create request: {ex.Message}");
            }
        }

        public ServiceResult CancelSyncRequest(int requestId, int userId)
        {
            try
            {
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Only allow cancellation of own requests
                if (request.UserId != userId)
                {
                    return ServiceResult.FailureResult("Not authorized");
                }

                // Only allow cancellation of pending requests (IsSuccessful == null)
                if (request.IsSuccessful != null)
                {
                    return ServiceResult.FailureResult("Cannot cancel processed request");
                }

                // Mark as cancelled (IsSuccessful = false)
                request.IsSuccessful = false;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.AttandanceSyncRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request cancelled");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to cancel request: {ex.Message}");
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
                        Status = GetStatusText(r.IsSuccessful)
                    })
                    .ToList();

                return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(requests);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<StatusDto>>.FailureResult($"Failed to retrieve statuses: {ex.Message}");
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
                return ServiceResult<IEnumerable<UserDto>>.FailureResult($"Failed to retrieve users: {ex.Message}");
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
                return ServiceResult<IEnumerable<SyncCompany>>.FailureResult($"Failed to retrieve companies: {ex.Message}");
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
                return ServiceResult<IEnumerable<Tool>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees()
        {
            try
            {
                var employees = _unitOfWork.Employees.GetActiveEmployees()
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        IsActive = e.IsActive
                    })
                    .ToList();

                return ServiceResult<IEnumerable<EmployeeDto>>.SuccessResult(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<EmployeeDto>>.FailureResult($"Failed to retrieve employees: {ex.Message}");
            }
        }

        private static string GetStatusText(bool? isSuccessful)
        {
            if (isSuccessful == null) return "Pending";
            return isSuccessful.Value ? "Completed" : "Failed";
        }
    }
}
