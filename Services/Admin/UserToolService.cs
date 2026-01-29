using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing tool assignments to users.
    /// Handles assignment, revocation, and retrieval of user-tool relationships and access control.
    /// </summary>
    public class UserToolService : IUserToolService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new UserToolService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public UserToolService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all tool assignments with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of tool assignments with user and tool details.</returns>
        public ServiceResult<PagedResultDto<UserToolDto>> GetAllAssignmentsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.UserTools.GetTotalAssignmentsCount();

                // Retrieve paginated assignments and map to DTOs
                var assignments = _unitOfWork.UserTools.GetAllAssignments(page, pageSize)
                    .Select(ut => new UserToolDto
                    {
                        Id = ut.Id,
                        UserId = ut.UserId,
                        UserName = ut.User?.Name ?? "Unknown",
                        UserEmail = ut.User?.Email ?? "Unknown",
                        ToolId = ut.ToolId,
                        ToolName = ut.Tool?.Name ?? "Unknown",
                        AssignedByName = ut.AssignedByUser?.Name ?? "Unknown",
                        AssignedAt = ut.AssignedAt,
                        IsRevoked = ut.IsRevoked,
                        RevokedAt = ut.RevokedAt
                    })
                    .ToList();

                var result = new PagedResultDto<UserToolDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = assignments
                };

                return ServiceResult<PagedResultDto<UserToolDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<UserToolDto>>.FailureResult($"Failed to retrieve assignments: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active tools assigned to a specific user.
        /// Maps tools to their corresponding route URLs and implementation status.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of assigned tools with routing information.</returns>
        public ServiceResult<IEnumerable<AssignedToolDto>> GetUserAssignedTools(int userId)
        {
            try
            {
                // Get active tool assignments for user
                var userTools = _unitOfWork.UserTools.GetActiveToolsByUserId(userId).ToList();

                // Map to DTOs with route information
                var assignments = userTools.Select(ut => new AssignedToolDto
                {
                    ToolId = ut.ToolId,
                    ToolName = ut.Tool.Name,
                    ToolDescription = ut.Tool.Description,
                    RouteUrl = GetToolRouteUrl(ut.Tool.Name),
                    IsImplemented = !ut.Tool.IsUnderDevelopment || ut.Tool.Name.Contains("Branch Issue")
                }).ToList();

                return ServiceResult<IEnumerable<AssignedToolDto>>.SuccessResult(assignments);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<AssignedToolDto>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        /// <summary>
        /// Maps tool names to their corresponding route URLs in the application.
        /// </summary>
        /// <param name="toolName">The name of the tool.</param>
        /// <returns>The route URL for the tool, or null if not mapped.</returns>
        private string GetToolRouteUrl(string toolName)
        {
            if (string.IsNullOrWhiteSpace(toolName))
                return null;

            // Normalize tool name for comparison
            var normalizedName = toolName.ToLower().Replace(" ", "");

            // Map tool names to routes
            if (normalizedName.Contains("attendance") || normalizedName.Contains("attandance"))
                return "~/Attandance/Index";

            if (normalizedName.Contains("salary") || normalizedName.Contains("garbge") || normalizedName.Contains("garbage"))
                return "~/SalaryGarbge/Index";

            if (normalizedName.Contains("concurrent") || normalizedName.Contains("simulation"))
                return "~/ConcurrentSimulation/Index";

            if (normalizedName.Contains("branch") && normalizedName.Contains("issue"))
                return "~/BranchIssue/Index";

            return null;
        }

        /// <summary>
        /// Assigns a tool to a user.
        /// Validates that both user and tool exist and are active before creating assignment.
        /// </summary>
        /// <param name="dto">Assignment details including user and tool IDs.</param>
        /// <param name="assignedBy">The admin user ID performing the assignment.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult AssignToolToUser(UserToolAssignDto dto, int assignedBy)
        {
            try
            {
                // Validate user exists and is active
                var user = _unitOfWork.Users.GetById(dto.UserId);
                if (user == null || !user.IsActive)
                {
                    return ServiceResult.FailureResult("User not found or inactive");
                }

                // Validate tool exists and is active
                var tool = _unitOfWork.Tools.GetById(dto.ToolId);
                if (tool == null || !tool.IsActive)
                {
                    return ServiceResult.FailureResult("Tool not found or inactive");
                }

                // Check if already assigned (active)
                if (_unitOfWork.UserTools.HasActiveAssignment(dto.UserId, dto.ToolId))
                {
                    return ServiceResult.FailureResult("Tool already assigned to this user");
                }

                // Create new assignment
                var userTool = new UserTool
                {
                    UserId = dto.UserId,
                    ToolId = dto.ToolId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.Now,
                    IsRevoked = false,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.UserTools.Add(userTool);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool assigned successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to assign tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Revokes a tool assignment from a user.
        /// Marks the assignment as revoked rather than deleting it.
        /// </summary>
        /// <param name="dto">Revocation details including user and tool IDs.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult RevokeToolFromUser(UserToolRevokeDto dto)
        {
            try
            {
                // Get active assignment
                var assignment = _unitOfWork.UserTools.GetActiveAssignment(dto.UserId, dto.ToolId);
                if (assignment == null)
                {
                    return ServiceResult.FailureResult("Assignment not found");
                }

                // Mark as revoked
                assignment.IsRevoked = true;
                assignment.RevokedAt = DateTime.Now;
                assignment.UpdatedAt = DateTime.Now;

                _unitOfWork.UserTools.Update(assignment);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool access revoked");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to revoke tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores a previously revoked tool assignment.
        /// Updates the assignment timestamp and assigned-by information.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="toolId">The tool ID.</param>
        /// <param name="assignedBy">The admin user ID performing the restoration.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UnrevokeToolAssignment(int userId, int toolId, int assignedBy)
        {
            try
            {
                // Find the revoked assignment
                var assignment = _unitOfWork.UserTools.FirstOrDefault(ut =>
                    ut.UserId == userId &&
                    ut.ToolId == toolId &&
                    ut.IsRevoked);

                if (assignment == null)
                {
                    return ServiceResult.FailureResult("Revoked assignment not found");
                }

                // Restore assignment
                assignment.IsRevoked = false;
                assignment.RevokedAt = null;
                assignment.AssignedBy = assignedBy;
                assignment.AssignedAt = DateTime.Now;
                assignment.UpdatedAt = DateTime.Now;

                _unitOfWork.UserTools.Update(assignment);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool access restored");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to restore tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active tool assignments for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of active tool assignments.</returns>
        public ServiceResult<IEnumerable<UserToolDto>> GetToolAssignmentsByUserId(int userId)
        {
            try
            {
                // Get active assignments
                var assignments = _unitOfWork.UserTools.GetActiveToolsByUserId(userId)
                    .Select(ut => new UserToolDto
                    {
                        Id = ut.Id,
                        UserId = ut.UserId,
                        UserName = ut.User?.Name ?? "Unknown",
                        ToolId = ut.ToolId,
                        ToolName = ut.Tool?.Name ?? "Unknown",
                        AssignedByName = ut.AssignedByUser?.Name ?? "Unknown",
                        AssignedAt = ut.AssignedAt,
                        IsRevoked = ut.IsRevoked
                    })
                    .ToList();

                return ServiceResult<IEnumerable<UserToolDto>>.SuccessResult(assignments);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UserToolDto>>.FailureResult($"Failed to retrieve assignments: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a user has active access to a specific tool.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="toolId">The tool ID.</param>
        /// <returns>True if user has active access, false otherwise.</returns>
        public bool UserHasToolAccess(int userId, int toolId)
        {
            // Check for active assignment
            return _unitOfWork.UserTools.HasActiveAssignment(userId, toolId);
        }
    }
}
