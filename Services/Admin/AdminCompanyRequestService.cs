using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing company access requests from an administrative perspective.
    /// Handles request retrieval, status updates, approval/rejection workflows,
    /// and database assignment operations for company requests.
    /// </summary>
    public class AdminCompanyRequestService : IAdminCompanyRequestService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new AdminCompanyRequestService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public AdminCompanyRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all company requests with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of company requests with user, employee, company, and tool details.</returns>
        public ServiceResult<PagedResultDto<CompanyRequestListDto>> GetAllRequestsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.CompanyRequests.GetTotalCount();

                // Retrieve paginated requests and map to DTOs
                var requests = _unitOfWork.CompanyRequests.GetPaged(page, pageSize)
                    .Select(r => new CompanyRequestListDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = r.User?.Name ?? "Unknown",
                        UserEmail = r.User?.Email ?? "Unknown",
                        EmployeeId = r.EmployeeId,
                        EmployeeName = r.Employee?.Name ?? "Unknown",
                        CompanyId = r.CompanyId,
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolId = r.ToolId,
                        ToolName = r.Tool?.Name ?? "Unknown",
                        Status = r.Status,
                        StatusText = GetStatusText(r.Status),
                        IsCancelled = r.IsCancelled,
                        CanProcess = !r.IsCancelled && r.Status != "CP" && r.Status != "RR",
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<CompanyRequestListDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<CompanyRequestListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<CompanyRequestListDto>>.FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific company request by its ID with full details.
        /// </summary>
        /// <param name="id">The request ID.</param>
        /// <returns>Company request details including user, employee, company, and tool information.</returns>
        public ServiceResult<CompanyRequestListDto> GetRequestById(int id)
        {
            try
            {
                // Fetch request with related entities
                var request = _unitOfWork.CompanyRequests.GetWithDetails(id);
                if (request == null)
                {
                    return ServiceResult<CompanyRequestListDto>.FailureResult("Request not found");
                }

                var dto = new CompanyRequestListDto
                {
                    Id = request.Id,
                    UserId = request.UserId,
                    UserName = request.User?.Name ?? "Unknown",
                    UserEmail = request.User?.Email ?? "Unknown",
                    EmployeeId = request.EmployeeId,
                    EmployeeName = request.Employee?.Name ?? "Unknown",
                    CompanyId = request.CompanyId,
                    CompanyName = request.Company?.Name ?? "Unknown",
                    ToolId = request.ToolId,
                    ToolName = request.Tool?.Name ?? "Unknown",
                    Status = request.Status,
                    StatusText = GetStatusText(request.Status),
                    IsCancelled = request.IsCancelled,
                    CanProcess = !request.IsCancelled && request.Status != "CP" && request.Status != "RR",
                    CreatedAt = request.CreatedAt,
                    UpdatedAt = request.UpdatedAt
                };

                return ServiceResult<CompanyRequestListDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<CompanyRequestListDto>.FailureResult($"Failed to retrieve request: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the status of a company request.
        /// Valid statuses: NR (New Request), IP (In Progress), CP (Completed), RR (Rejected).
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="status">The new status code.</param>
        /// <returns>Success or failure result with appropriate message.</returns>
        public ServiceResult UpdateRequestStatus(int requestId, string status)
        {
            try
            {
                // Validate status against allowed values
                var validStatuses = new[] { "NR", "IP", "CP", "RR" };
                if (!validStatuses.Contains(status))
                {
                    return ServiceResult.FailureResult("Invalid status. Valid values are: NR, IP, CP, RR");
                }

                var request = _unitOfWork.CompanyRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Check if request is cancelled
                if (request.IsCancelled)
                {
                    return ServiceResult.FailureResult("Cannot update status of a cancelled request");
                }

                // Check if request is already completed or rejected
                if (request.Status == "CP" || request.Status == "RR")
                {
                    return ServiceResult.FailureResult("Cannot update status of a completed or rejected request");
                }

                request.Status = status;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult($"Request status updated to {GetStatusText(status)}");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update request status: {ex.Message}");
            }
        }

        /// <summary>
        /// Accepts a new company request and moves it to In Progress status.
        /// Only new requests (NR) can be accepted.
        /// </summary>
        /// <param name="requestId">The request ID to accept.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult AcceptRequest(int requestId)
        {
            try
            {
                // Retrieve the request
                var request = _unitOfWork.CompanyRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                if (request.IsCancelled)
                {
                    return ServiceResult.FailureResult("Cannot accept a cancelled request");
                }

                if (request.Status != "NR")
                {
                    return ServiceResult.FailureResult("Only new requests can be accepted");
                }

                request.Status = "IP";
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request accepted and set to In Progress");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to accept request: {ex.Message}");
            }
        }

        /// <summary>
        /// Rejects a company request and marks it as rejected (RR).
        /// Cannot reject already completed or rejected requests.
        /// </summary>
        /// <param name="requestId">The request ID to reject.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult RejectRequest(int requestId)
        {
            try
            {
                // Retrieve the request
                var request = _unitOfWork.CompanyRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                if (request.IsCancelled)
                {
                    return ServiceResult.FailureResult("Cannot reject a cancelled request");
                }

                if (request.Status == "CP" || request.Status == "RR")
                {
                    return ServiceResult.FailureResult("Cannot reject a completed or already rejected request");
                }

                request.Status = "RR";
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request rejected");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to reject request: {ex.Message}");
            }
        }

        /// <summary>
        /// Assigns a database configuration to a company request and marks it as completed.
        /// Creates a DatabaseAssign record linking the request to the company's database configuration.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="adminUserId">The admin user ID performing the assignment.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult AssignDatabase(int requestId, int adminUserId)
        {
            try
            {
                // Retrieve request with full details
                var request = _unitOfWork.CompanyRequests.GetWithDetails(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                if (request.IsCancelled)
                {
                    return ServiceResult.FailureResult("Cannot assign database to a cancelled request");
                }

                if (request.Status == "RR" || request.Status == "CP")
                {
                    return ServiceResult.FailureResult("Cannot assign database to a rejected or completed request");
                }

                // Check if already assigned
                if (_unitOfWork.DatabaseAssignments.HasAssignment(requestId))
                {
                    return ServiceResult.FailureResult("Database already assigned to this request");
                }

                // Get the database configuration for the request's company
                var dbConfig = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (dbConfig == null)
                {
                    return ServiceResult.FailureResult($"No database configuration found for company '{request.Company?.Name ?? "Unknown"}'");
                }

                // Create assignment
                var assignment = new DatabaseAssign
                {
                    CompanyRequestId = requestId,
                    DatabaseConfigurationId = dbConfig.Id,
                    AssignedBy = adminUserId,
                    AssignedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.DatabaseAssignments.Add(assignment);

                // Update request status to Completed
                request.Status = "CP";
                request.UpdatedAt = DateTime.Now;
                _unitOfWork.CompanyRequests.Update(request);

                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database assigned successfully. Request marked as completed.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to assign database: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the database configuration for a company request's associated company.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <returns>Database configuration details for the company.</returns>
        public ServiceResult<DatabaseConfigDto> GetDatabaseConfigForRequest(int requestId)
        {
            try
            {
                // Get request details
                var request = _unitOfWork.CompanyRequests.GetWithDetails(requestId);
                if (request == null)
                {
                    return ServiceResult<DatabaseConfigDto>.FailureResult("Request not found");
                }

                var dbConfig = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (dbConfig == null)
                {
                    return ServiceResult<DatabaseConfigDto>.FailureResult($"No database configuration found for company '{request.Company?.Name ?? "Unknown"}'");
                }

                var dto = new DatabaseConfigDto
                {
                    Id = dbConfig.Id,
                    CompanyId = dbConfig.CompanyId,
                    CompanyName = request.Company?.Name ?? "Unknown",
                    DatabaseIP = dbConfig.DatabaseIP,
                    DatabaseName = dbConfig.DatabaseName,
                    DatabaseUserId = dbConfig.DatabaseUserId,
                    CreatedAt = dbConfig.CreatedAt,
                    UpdatedAt = dbConfig.UpdatedAt
                };

                return ServiceResult<DatabaseConfigDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<DatabaseConfigDto>.FailureResult($"Failed to get database configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the count of new requests created after a specific request ID.
        /// Used for real-time notification of new requests.
        /// </summary>
        /// <param name="lastKnownId">The last known request ID.</param>
        /// <returns>Count of new requests.</returns>
        public ServiceResult<int> GetNewRequestsCount(int lastKnownId)
        {
            try
            {
                // Count requests with IDs greater than the last known
                var count = _unitOfWork.CompanyRequests.Count(r => r.Id > lastKnownId);
                return ServiceResult<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to get count: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the ID of the most recently created request.
        /// </summary>
        /// <returns>The newest request ID, or 0 if no requests exist.</returns>
        public ServiceResult<int> GetNewestRequestId()
        {
            try
            {
                // Get the highest request ID
                var newest = _unitOfWork.CompanyRequests.GetAll()
                    .OrderByDescending(r => r.Id)
                    .Select(r => r.Id)
                    .FirstOrDefault();
                return ServiceResult<int>.SuccessResult(newest);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to get newest ID: {ex.Message}");
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
