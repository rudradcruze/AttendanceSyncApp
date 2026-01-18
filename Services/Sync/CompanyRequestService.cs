using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;

namespace AttandanceSyncApp.Services.Sync
{
    public class CompanyRequestService : ICompanyRequestService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public CompanyRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<CompanyRequestDto>> GetUserRequestsPaged(int userId, int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.CompanyRequests.GetTotalCountByUserId(userId);
                var requests = _unitOfWork.CompanyRequests.GetPagedByUserId(userId, page, pageSize)
                    .Select(r => new CompanyRequestDto
                    {
                        Id = r.Id,
                        UserName = r.User?.Name ?? "Unknown",
                        EmployeeName = r.Employee?.Name ?? "Unknown",
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolName = r.Tool?.Name ?? "Unknown",
                        Status = r.Status,
                        StatusText = GetStatusText(r.Status),
                        IsCancelled = r.IsCancelled,
                        CanCancel = r.Status == "NR" && !r.IsCancelled,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<CompanyRequestDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<CompanyRequestDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<CompanyRequestDto>>.FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        public ServiceResult<int> CreateRequest(CompanyRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
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

                // Create company request
                var request = new CompanyRequest
                {
                    UserId = userId,
                    EmployeeId = dto.EmployeeId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId,
                    SessionId = sessionId,
                    Status = "NR", // New Request
                    IsCancelled = false,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.CompanyRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(request.Id, "Request created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to create request: {ex.Message}");
            }
        }

        public ServiceResult CancelRequest(int requestId, int userId)
        {
            try
            {
                var request = _unitOfWork.CompanyRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Only allow cancellation of own requests
                if (request.UserId != userId)
                {
                    return ServiceResult.FailureResult("Not authorized");
                }

                // Only allow cancellation if Status is NR and not already cancelled
                if (request.Status != "NR")
                {
                    return ServiceResult.FailureResult("Cannot cancel request that is already in progress or completed");
                }

                if (request.IsCancelled)
                {
                    return ServiceResult.FailureResult("Request is already cancelled");
                }

                // Mark as cancelled
                request.IsCancelled = true;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request cancelled successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to cancel request: {ex.Message}");
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

        private static string GetStatusText(string status)
        {
            switch (status)
            {
                case "NR": return "New Request";
                case "IP": return "In Progress";
                case "CP": return "Completed";
                case "RR": return "Rejected";
                default: return status;
            }
        }
    }
}
