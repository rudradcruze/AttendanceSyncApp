using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing attendance synchronization requests from an administrative perspective.
    /// Handles retrieval, filtering, and status updates of sync requests.
    /// </summary>
    public class AdminRequestService : IAdminRequestService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new AdminRequestService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public AdminRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all sync requests with pagination.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>Paginated list of sync requests.</returns>
        public ServiceResult<PagedResultDto<RequestListDto>> GetAllRequestsPaged(int page, int pageSize)
        {
            // Delegate to filtered method with minimal filter
            return GetRequestsFiltered(new RequestFilterDto { Page = page, PageSize = pageSize });
        }

        /// <summary>
        /// Retrieves sync requests with advanced filtering options.
        /// </summary>
        /// <param name="filter">Filter criteria including user search, company, status, and date range.</param>
        /// <returns>Paginated list of filtered sync requests.</returns>
        public ServiceResult<PagedResultDto<RequestListDto>> GetRequestsFiltered(RequestFilterDto filter)
        {
            try
            {
                int totalCount;
                // Apply filters and retrieve paginated results
                var requests = _unitOfWork.AttandanceSyncRequests.GetFiltered(
                    filter.UserSearch,
                    filter.CompanyId,
                    filter.Status,
                    filter.FromDate,
                    filter.ToDate,
                    filter.Page,
                    filter.PageSize,
                    out totalCount
                )
                .Select(r => new RequestListDto
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
                    SessionId = r.SessionId,
                    ExternalSyncId = r.ExternalSyncId,
                    IsSuccessful = r.IsSuccessful,
                    Status = GetStatusText(r.IsSuccessful),
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasDatabaseConfig = r.DatabaseConfiguration != null
                })
                .ToList();

                var result = new PagedResultDto<RequestListDto>
                {
                    TotalRecords = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<RequestListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<RequestListDto>>.FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific sync request by its ID with configuration details.
        /// </summary>
        /// <param name="id">The request ID.</param>
        /// <returns>Sync request details including user, employee, company, and tool information.</returns>
        public ServiceResult<RequestListDto> GetRequestById(int id)
        {
            try
            {
                // Fetch request with database configuration
                var request = _unitOfWork.AttandanceSyncRequests.GetWithConfiguration(id);
                if (request == null)
                {
                    return ServiceResult<RequestListDto>.FailureResult("Request not found");
                }

                var requestDto = new RequestListDto
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
                    SessionId = request.SessionId,
                    ExternalSyncId = request.ExternalSyncId,
                    IsSuccessful = request.IsSuccessful,
                    Status = GetStatusText(request.IsSuccessful),
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    CreatedAt = request.CreatedAt,
                    UpdatedAt = request.UpdatedAt,
                    HasDatabaseConfig = request.DatabaseConfiguration != null
                };

                return ServiceResult<RequestListDto>.SuccessResult(requestDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<RequestListDto>.FailureResult($"Failed to retrieve request: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the status of a sync request.
        /// Maps status strings to IsSuccessful boolean field.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="status">The status string (e.g., COMPLETED, FAILED, PENDING).</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateRequestStatus(int requestId, string status)
        {
            try
            {
                // Retrieve the request
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Map status text to IsSuccessful nullable boolean
                bool? isSuccessful = null;
                switch (status?.ToUpper())
                {
                    case "COMPLETED":
                    case "CP":
                    case "SUCCESS":
                        isSuccessful = true;
                        break;
                    case "FAILED":
                    case "CANCELLED":
                        isSuccessful = false;
                        break;
                    case "PENDING":
                    case "NR":
                    case "IP":
                        isSuccessful = null;
                        break;
                    default:
                        return ServiceResult.FailureResult("Invalid status");
                }

                request.IsSuccessful = isSuccessful;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.AttandanceSyncRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request updated");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update request: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a sync request by updating its external sync ID and success status.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="externalSyncId">The external synchronization ID from the target system.</param>
        /// <param name="isSuccessful">Whether the synchronization was successful.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult ProcessRequest(int requestId, int? externalSyncId, bool isSuccessful)
        {
            try
            {
                // Retrieve the request
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                request.ExternalSyncId = externalSyncId;
                request.IsSuccessful = isSuccessful;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.AttandanceSyncRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request processed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to process request: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts IsSuccessful boolean to human-readable status text.
        /// </summary>
        /// <param name="isSuccessful">The success status (null = Pending, true = Completed, false = Failed).</param>
        /// <returns>Status text.</returns>
        private static string GetStatusText(bool? isSuccessful)
        {
            // Map nullable boolean to status text
            if (isSuccessful == null) return "Pending";
            return isSuccessful.Value ? "Completed" : "Failed";
        }
    }
}
