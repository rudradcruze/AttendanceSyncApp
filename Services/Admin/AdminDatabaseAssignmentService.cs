using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing database assignments to company requests.
    /// Handles retrieval of assignments and revocation/un-revocation operations.
    /// </summary>
    public class AdminDatabaseAssignmentService : IAdminDatabaseAssignmentService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new AdminDatabaseAssignmentService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public AdminDatabaseAssignmentService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all database assignments with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of database assignments with related details.</returns>
        public ServiceResult<PagedResultDto<DatabaseAssignListDto>> GetAllAssignmentsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
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

        /// <summary>
        /// Retrieves a specific database assignment by its ID with full details.
        /// </summary>
        /// <param name="id">The assignment ID.</param>
        /// <returns>Database assignment details with user, company, and configuration information.</returns>
        public ServiceResult<DatabaseAssignListDto> GetAssignmentById(int id)
        {
            try
            {
                // Fetch assignment with related entities
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

        /// <summary>
        /// Retrieves the database assignment for a specific company request.
        /// </summary>
        /// <param name="companyRequestId">The company request ID.</param>
        /// <returns>Database assignment details for the request.</returns>
        public ServiceResult<DatabaseAssignListDto> GetAssignmentByRequestId(int companyRequestId)
        {
            try
            {
                // Find assignment by company request ID
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

        /// <summary>
        /// Revokes a database assignment, preventing access.
        /// </summary>
        /// <param name="id">The assignment ID to revoke.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult RevokeAssignment(int id)
        {
            try
            {
                // Retrieve the assignment
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

        /// <summary>
        /// Restores a revoked database assignment, re-enabling access.
        /// </summary>
        /// <param name="id">The assignment ID to un-revoke.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UnrevokeAssignment(int id)
        {
            try
            {
                // Retrieve the assignment
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

        /// <summary>
        /// Maps a DatabaseAssign entity to a DatabaseAssignListDto.
        /// </summary>
        /// <param name="a">The database assignment entity.</param>
        /// <returns>The mapped DTO with all related information.</returns>
        private static DatabaseAssignListDto MapToDto(DatabaseAssign a)
        {
            // Map assignment to DTO with navigation properties
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
