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
            try
            {
                var totalCount = _unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var requests = _unitOfWork.AttandanceSyncRequests.GetPaged(page, pageSize)
                    .Select(r => new RequestListDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        UserName = r.User?.Name ?? "Unknown",
                        UserEmail = r.User?.Email ?? "Unknown",
                        CompanyId = r.CompanyId,
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolId = r.ToolId,
                        ToolName = r.Tool?.Name ?? "Unknown",
                        Email = r.Email,
                        Status = r.Status,
                        FromDate = r.FromDate,
                        ToDate = r.ToDate,
                        CreatedAt = r.CreatedAt,
                        HasDatabaseConfig = r.DatabaseConfiguration != null
                    })
                    .ToList();

                var result = new PagedResultDto<RequestListDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<RequestListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<RequestListDto>>.FailureResult($"Error retrieving requests: {ex.Message}");
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
                    CompanyId = request.CompanyId,
                    CompanyName = request.Company?.Name ?? "Unknown",
                    ToolId = request.ToolId,
                    ToolName = request.Tool?.Name ?? "Unknown",
                    Email = request.Email,
                    Status = request.Status,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    CreatedAt = request.CreatedAt,
                    HasDatabaseConfig = request.DatabaseConfiguration != null
                };

                return ServiceResult<RequestListDto>.SuccessResult(requestDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<RequestListDto>.FailureResult($"Error retrieving request: {ex.Message}");
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

                var validStatuses = new[] { "NR", "IP", "CP" };
                if (!validStatuses.Contains(status))
                {
                    return ServiceResult.FailureResult("Invalid status. Must be NR, IP, or CP.");
                }

                request.Status = status;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.AttandanceSyncRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request status updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error updating request status: {ex.Message}");
            }
        }
    }
}
