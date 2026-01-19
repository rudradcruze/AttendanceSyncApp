using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class AdminDatabaseAssignmentService : IAdminDatabaseAssignmentService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminDatabaseAssignmentService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<DatabaseAssignListDto>> GetAllAssignmentsPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.DatabaseAssignments.GetTotalCount();
                var assignments = _unitOfWork.DatabaseAssignments.GetPaged(page, pageSize)
                    .Select(a => MapToDto(a))
                    .ToList();

                var result = new PagedResultDto<DatabaseAssignListDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = assignments
                };

                return ServiceResult<PagedResultDto<DatabaseAssignListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<DatabaseAssignListDto>>.FailureResult($"Failed to retrieve assignments: {ex.Message}");
            }
        }

        public ServiceResult<DatabaseAssignListDto> GetAssignmentById(int id)
        {
            try
            {
                var assignment = _unitOfWork.DatabaseAssignments.GetWithDetails(id);
                if (assignment == null)
                {
                    return ServiceResult<DatabaseAssignListDto>.FailureResult("Assignment not found");
                }

                return ServiceResult<DatabaseAssignListDto>.SuccessResult(MapToDto(assignment));
            }
            catch (Exception ex)
            {
                return ServiceResult<DatabaseAssignListDto>.FailureResult($"Failed to retrieve assignment: {ex.Message}");
            }
        }

        public ServiceResult<DatabaseAssignListDto> GetAssignmentByRequestId(int companyRequestId)
        {
            try
            {
                var assignment = _unitOfWork.DatabaseAssignments.GetByCompanyRequestId(companyRequestId);
                if (assignment == null)
                {
                    return ServiceResult<DatabaseAssignListDto>.FailureResult("Assignment not found for this request");
                }

                var detailed = _unitOfWork.DatabaseAssignments.GetWithDetails(assignment.Id);
                return ServiceResult<DatabaseAssignListDto>.SuccessResult(MapToDto(detailed));
            }
            catch (Exception ex)
            {
                return ServiceResult<DatabaseAssignListDto>.FailureResult($"Failed to retrieve assignment: {ex.Message}");
            }
        }

        public ServiceResult RevokeAssignment(int id)
        {
            try
            {
                var assignment = _unitOfWork.DatabaseAssignments.GetById(id);
                if (assignment == null)
                {
                    return ServiceResult.FailureResult("Assignment not found");
                }

                if (assignment.IsRevoked)
                {
                    return ServiceResult.FailureResult("Assignment is already revoked");
                }

                assignment.IsRevoked = true;
                assignment.RevokedAt = DateTime.Now;
                assignment.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAssignments.Update(assignment);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database assignment revoked successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to revoke assignment: {ex.Message}");
            }
        }

        public ServiceResult UnrevokeAssignment(int id)
        {
            try
            {
                var assignment = _unitOfWork.DatabaseAssignments.GetById(id);
                if (assignment == null)
                {
                    return ServiceResult.FailureResult("Assignment not found");
                }

                if (!assignment.IsRevoked)
                {
                    return ServiceResult.FailureResult("Assignment is not revoked");
                }

                assignment.IsRevoked = false;
                assignment.RevokedAt = null;
                assignment.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseAssignments.Update(assignment);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database assignment un-revoked successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to un-revoke assignment: {ex.Message}");
            }
        }

        private static DatabaseAssignListDto MapToDto(DatabaseAssign a)
        {
            return new DatabaseAssignListDto
            {
                Id = a.Id,
                CompanyRequestId = a.CompanyRequestId,
                UserName = a.CompanyRequest?.User?.Name ?? "Unknown",
                UserEmail = a.CompanyRequest?.User?.Email ?? "Unknown",
                EmployeeName = a.CompanyRequest?.Employee?.Name ?? "Unknown",
                CompanyName = a.CompanyRequest?.Company?.Name ?? "Unknown",
                ToolName = a.CompanyRequest?.Tool?.Name ?? "Unknown",
                AssignedBy = a.AssignedBy,
                AssignedByName = a.AssignedByUser?.Name ?? "Unknown",
                AssignedAt = a.AssignedAt,
                DatabaseConfigurationId = a.DatabaseConfigurationId,
                DatabaseIP = a.DatabaseConfiguration?.DatabaseIP ?? "Unknown",
                DatabaseName = a.DatabaseConfiguration?.DatabaseName ?? "Unknown",
                DatabaseUserId = a.DatabaseConfiguration?.DatabaseUserId ?? "Unknown",
                IsRevoked = a.IsRevoked,
                RevokedAt = a.RevokedAt,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }
    }
}
