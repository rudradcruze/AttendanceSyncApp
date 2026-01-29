using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Services.AttandanceSync
{
    /// <summary>
    /// Service for managing company access requests from a user perspective.
    /// Handles request creation, cancellation, and retrieval of user-specific company access requests.
    /// </summary>
    public class CompanyRequestService : ICompanyRequestService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new CompanyRequestService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public CompanyRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves company requests for a specific user with pagination support.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of user's company requests.</returns>
        public ServiceResult<PagedResultDto<CompanyRequestDto>> GetUserRequestsPaged(int userId, int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.CompanyRequests.GetTotalCountByUserId(userId);

                // Retrieve paginated requests and map to DTOs
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

        /// <summary>
        /// Creates a new company access request for a user.
        /// Validates employee, company, and tool existence before creation.
        /// </summary>
        /// <param name="dto">Request details including employee, company, and tool IDs.</param>
        /// <param name="userId">The user ID creating the request.</param>
        /// <param name="sessionId">The session ID associated with the request.</param>
        /// <returns>The ID of the created request.</returns>
        public ServiceResult<int> CreateRequest(CompanyRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate employee exists and is active
                var employee = _unitOfWork.Employees.GetById(dto.EmployeeId);
                if (employee == null || !employee.IsActive)
                {
                    return ServiceResult<int>.FailureResult("Selected employee not found or inactive");
                }

                // Validate company exists and is active
                var company = _unitOfWork.SyncCompanies.GetById(dto.CompanyId);
                if (company == null || company.Status != "Active")
                {
                    return ServiceResult<int>.FailureResult("Selected company not found or inactive");
                }

                // Validate tool exists and is active
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

        /// <summary>
        /// Cancels a user's company request.
        /// Only allows cancellation of new requests that haven't been processed.
        /// </summary>
        /// <param name="requestId">The request ID to cancel.</param>
        /// <param name="userId">The user ID requesting cancellation.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult CancelRequest(int requestId, int userId)
        {
            try
            {
                // Validate request exists
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

        /// <summary>
        /// Retrieves all active employees for request creation dropdowns.
        /// </summary>
        /// <returns>List of active employees.</returns>
        public ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees()
        {
            try
            {
                // Get active employees
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

        /// <summary>
        /// Retrieves all active companies for request creation dropdowns.
        /// </summary>
        /// <returns>List of active companies.</returns>
        public ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies()
        {
            try
            {
                // Get active companies
                var companies = _unitOfWork.SyncCompanies.GetActiveCompanies();
                return ServiceResult<IEnumerable<SyncCompany>>.SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<SyncCompany>>.FailureResult($"Failed to retrieve companies: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active tools for request creation dropdowns.
        /// </summary>
        /// <returns>List of active tools.</returns>
        public ServiceResult<IEnumerable<Tool>> GetActiveTools()
        {
            try
            {
                // Get active tools
                var tools = _unitOfWork.Tools.GetActiveTools();
                return ServiceResult<IEnumerable<Tool>>.SuccessResult(tools);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Tool>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts status codes to human-readable text.
        /// </summary>
        /// <param name="status">The status code.</param>
        /// <returns>Human-readable status description.</returns>
        private static string GetStatusText(string status)
        {
            // Map status codes to display text
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
