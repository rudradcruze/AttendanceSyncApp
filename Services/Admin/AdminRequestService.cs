using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class AdminRequestService : IAdminRequestService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<RequestListDto>> GetAllRequestsPaged(int page, int pageSize)
        {
            return GetRequestsFiltered(new RequestFilterDto { Page = page, PageSize = pageSize });
        }

        public ServiceResult<PagedResultDto<RequestListDto>> GetRequestsFiltered(RequestFilterDto filter)
        {
            try
            {
                int totalCount;
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

        public ServiceResult<RequestListDto> GetRequestById(int id)
        {
            try
            {
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

        public ServiceResult UpdateRequestStatus(int requestId, string status)
        {
            try
            {
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Map status text to IsSuccessful
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

        public ServiceResult ProcessRequest(int requestId, int? externalSyncId, bool isSuccessful)
        {
            try
            {
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

        private static string GetStatusText(bool? isSuccessful)
        {
            if (isSuccessful == null) return "Pending";
            return isSuccessful.Value ? "Completed" : "Failed";
        }
    }
}
