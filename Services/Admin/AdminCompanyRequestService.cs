using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.CompanyRequest;
using AttandanceSyncApp.Models.Sync;
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
                        IsRevoked = r.IsRevoked,
                        RevokedAt = r.RevokedAt,
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
                    IsRevoked = request.IsRevoked,
                    RevokedAt = request.RevokedAt,
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

        public ServiceResult AcceptRequest(int requestId)
        {
            try
            {
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

        public ServiceResult RejectRequest(int requestId)
        {
            try
            {
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

        public ServiceResult RevokeConnection(int requestId)
        {
            try
            {
                var request = _unitOfWork.CompanyRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                if (request.Status != "CP")
                {
                    return ServiceResult.FailureResult("Only completed requests can be revoked");
                }

                if (request.IsRevoked)
                {
                    return ServiceResult.FailureResult("Connection already revoked");
                }

                request.IsRevoked = true;
                request.RevokedAt = DateTime.Now;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.CompanyRequests.Update(request);
                
                // Optionally remove assignment? 
                // The user said "Revoke the database connection", usually implies breaking the link.
                // But the request remains "CP" (Completed) just revoked.
                // Depending on requirements, we might want to delete DatabaseAssignment.
                // For now, I'll just set the flag as requested.

                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Connection revoked successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to revoke connection: {ex.Message}");
            }
        }

        public ServiceResult AssignDatabase(int requestId, int adminUserId)
        {
            try
            {
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

        public ServiceResult<DatabaseConfigDto> GetDatabaseConfigForRequest(int requestId)
        {
            try
            {
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

        public ServiceResult<int> GetNewRequestsCount(int lastKnownId)
        {
            try
            {
                var count = _unitOfWork.CompanyRequests.Count(r => r.Id > lastKnownId);
                return ServiceResult<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to get count: {ex.Message}");
            }
        }

        public ServiceResult<int> GetNewestRequestId()
        {
            try
            {
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
