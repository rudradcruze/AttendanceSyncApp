using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class UserToolService : IUserToolService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        // Define which tools are implemented
        private readonly HashSet<string> _implementedTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Attendance Sync"
        };

        // Map tool names to their route URLs
        private readonly Dictionary<string, string> _toolRoutes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Attendance Sync", "~/Attandance/Index" }
        };

        public UserToolService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<UserToolDto>> GetAllAssignmentsPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.UserTools.GetTotalAssignmentsCount();
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

        public ServiceResult<IEnumerable<AssignedToolDto>> GetUserAssignedTools(int userId)
        {
            try
            {
                var assignments = _unitOfWork.UserTools.GetActiveToolsByUserId(userId)
                    .Select(ut => new AssignedToolDto
                    {
                        ToolId = ut.ToolId,
                        ToolName = ut.Tool.Name,
                        ToolDescription = ut.Tool.Description,
                        RouteUrl = GetToolRouteUrl(ut.Tool.Name),
                        IsImplemented = _implementedTools.Contains(ut.Tool.Name)
                    })
                    .ToList();

                return ServiceResult<IEnumerable<AssignedToolDto>>.SuccessResult(assignments);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<AssignedToolDto>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        public ServiceResult AssignToolToUser(UserToolAssignDto dto, int assignedBy)
        {
            try
            {
                // Validate user exists
                var user = _unitOfWork.Users.GetById(dto.UserId);
                if (user == null || !user.IsActive)
                {
                    return ServiceResult.FailureResult("User not found or inactive");
                }

                // Validate tool exists
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

        public ServiceResult RevokeToolFromUser(UserToolRevokeDto dto)
        {
            try
            {
                var assignment = _unitOfWork.UserTools.GetActiveAssignment(dto.UserId, dto.ToolId);
                if (assignment == null)
                {
                    return ServiceResult.FailureResult("Assignment not found");
                }

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

        public ServiceResult<IEnumerable<UserToolDto>> GetToolAssignmentsByUserId(int userId)
        {
            try
            {
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

        public bool UserHasToolAccess(int userId, int toolId)
        {
            return _unitOfWork.UserTools.HasActiveAssignment(userId, toolId);
        }

        private string GetToolRouteUrl(string toolName)
        {
            return _toolRoutes.TryGetValue(toolName, out var url) ? url : null;
        }
    }
}
