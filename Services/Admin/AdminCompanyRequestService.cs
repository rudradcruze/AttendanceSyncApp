using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class AdminCompanyRequestService : IAdminCompanyRequestService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminCompanyRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<CompanyRequestListDto>> GetAllRequestsPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.CompanyRequests.GetTotalCount();
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

        public ServiceResult<CompanyRequestListDto> GetRequestById(int id)
        {
            try
            {
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

        public ServiceResult UpdateRequestStatus(int requestId, string status)
        {
            try
            {
                // Validate status
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
