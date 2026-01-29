using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;

namespace AttandanceSyncApp.Services.Sync
{
    /// <summary>
    /// Service responsible for handling company access requests
    /// from the end-user perspective. Manages request creation,
    /// cancellation, and retrieval of user-specific requests.
    /// </summary>
    public class CompanyRequestService : ICompanyRequestService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of CompanyRequestService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public CompanyRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves paginated company requests created by a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Records per page.</param>
        /// <returns>Paginated list of user requests.</returns>
        public ServiceResult<PagedResultDto<CompanyRequestDto>>
            GetUserRequestsPaged(int userId, int page, int pageSize)
        {
            try
            {
                // Retrieve total request count for pagination
                var totalCount =
                    _unitOfWork.CompanyRequests
                        .GetTotalCountByUserId(userId);

                // Retrieve paginated requests and map to DTOs
                var requests =
                    _unitOfWork.CompanyRequests
                        .GetPagedByUserId(userId, page, pageSize)
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

                            // User can cancel only new, active requests
                            CanCancel =
                                r.Status == "NR" && !r.IsCancelled,

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

                return ServiceResult<PagedResultDto<CompanyRequestDto>>
                    .SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<CompanyRequestDto>>
                    .FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new company access request for a user.
        /// </summary>
        /// <param name="dto">Request creation data.</param>
        /// <param name="userId">Requesting user ID.</param>
        /// <param name="sessionId">Current login session ID.</param>
        /// <returns>Created request ID.</returns>
        public ServiceResult<int>
            CreateRequest(CompanyRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate employee existence and active status
                var employee =
                    _unitOfWork.Employees.GetById(dto.EmployeeId);

                if (employee == null || !employee.IsActive)
                {
                    return ServiceResult<int>
                        .FailureResult("Selected employee not found or inactive");
                }

                // Validate company existence and active status
                var company =
                    _unitOfWork.SyncCompanies.GetById(dto.CompanyId);

                if (company == null || company.Status != "Active")
                {
                    return ServiceResult<int>
                        .FailureResult("Selected company not found or inactive");
                }

                // Validate tool existence and active status
                var tool =
                    _unitOfWork.Tools.GetById(dto.ToolId);

                if (tool == null || !tool.IsActive)
                {
                    return ServiceResult<int>
                        .FailureResult("Selected tool not found or inactive");
                }

                // Create new company request entity
                var request = new CompanyRequest
                {
                    UserId = userId,
                    EmployeeId = dto.EmployeeId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId,
                    SessionId = sessionId,
                    Status = "NR",      // New Request
                    IsCancelled = false,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.CompanyRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>
                    .SuccessResult(request.Id, "Request created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>
                    .FailureResult($"Failed to create request: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels a user-owned company request.
        /// </summary>
        /// <param name="requestId">Request ID.</param>
        /// <param name="userId">Requesting user ID.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult CancelRequest(int requestId, int userId)
        {
            try
            {
                // Retrieve request
                var request =
                    _unitOfWork.CompanyRequests.GetById(requestId);

                if (request == null)
                {
                    return ServiceResult
                        .FailureResult("Request not found");
                }

                // Ensure user owns the request
                if (request.UserId != userId)
                {
                    return ServiceResult
                        .FailureResult("Not authorized");
                }

                // Only new requests can be cancelled
                if (request.Status != "NR")
                {
                    return ServiceResult
                        .FailureResult("Cannot cancel request that is already in progress or completed");
                }

                if (request.IsCancelled)
                {
                    return ServiceResult
                        .FailureResult("Request is already cancelled");
                }

                // Mark request as cancelled
                request.IsCancelled = true;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult
                    .SuccessResult("Request cancelled successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to cancel request: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active employees available for request creation.
        /// </summary>
        /// <returns>List of active employees.</returns>
        public ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees()
        {
            try
            {
                var employees =
                    _unitOfWork.Employees.GetActiveEmployees()
                        .Select(e => new EmployeeDto
                        {
                            Id = e.Id,
                            Name = e.Name,
                            IsActive = e.IsActive
                        })
                        .ToList();

                return ServiceResult<IEnumerable<EmployeeDto>>
                    .SuccessResult(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<EmployeeDto>>
                    .FailureResult($"Failed to retrieve employees: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active companies available for requests.
        /// </summary>
        /// <returns>List of active companies.</returns>
        public ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies()
        {
            try
            {
                var companies =
                    _unitOfWork.SyncCompanies.GetActiveCompanies();

                return ServiceResult<IEnumerable<SyncCompany>>
                    .SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<SyncCompany>>
                    .FailureResult($"Failed to retrieve companies: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active tools available for requests.
        /// </summary>
        /// <returns>List of active tools.</returns>
        public ServiceResult<IEnumerable<Tool>> GetActiveTools()
        {
            try
            {
                var tools =
                    _unitOfWork.Tools.GetActiveTools();

                return ServiceResult<IEnumerable<Tool>>
                    .SuccessResult(tools);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Tool>>
                    .FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts request status codes into human-readable text.
        /// </summary>
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
