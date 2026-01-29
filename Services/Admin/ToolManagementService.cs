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
    /// Service for managing tool records from an administrative perspective.
    /// Handles CRUD operations for tools and their status/development flags.
    /// </summary>
    public class ToolManagementService : IToolManagementService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new ToolManagementService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public ToolManagementService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all tools with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of tools with their details.</returns>
        public ServiceResult<PagedResultDto<ToolDto>> GetToolsPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.Tools.Count();
                var tools = _unitOfWork.Tools.GetAll()
                    .OrderByDescending(t => t.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new ToolDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        IsActive = t.IsActive,
                        IsUnderDevelopment = t.IsUnderDevelopment,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<ToolDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = tools
                };

                return ServiceResult<PagedResultDto<ToolDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<ToolDto>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific tool by ID.
        /// </summary>
        /// <param name="id">The tool ID.</param>
        /// <returns>Tool details including name, description, and status.</returns>
        public ServiceResult<ToolDto> GetToolById(int id)
        {
            try
            {
                // Fetch tool by ID
                var tool = _unitOfWork.Tools.GetById(id);
                if (tool == null)
                {
                    return ServiceResult<ToolDto>.FailureResult("Tool not found");
                }

                var dto = new ToolDto
                {
                    Id = tool.Id,
                    Name = tool.Name,
                    Description = tool.Description,
                    IsActive = tool.IsActive,
                    IsUnderDevelopment = tool.IsUnderDevelopment,
                    CreatedAt = tool.CreatedAt,
                    UpdatedAt = tool.UpdatedAt
                };

                return ServiceResult<ToolDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ToolDto>.FailureResult($"Failed to retrieve tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new tool record.
        /// </summary>
        /// <param name="dto">The tool data to create.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult CreateTool(ToolCreateDto dto)
        {
            try
            {
                // Validate tool name
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Tool name is required");
                }

                var tool = new Tool
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive,
                    IsUnderDevelopment = dto.IsUnderDevelopment,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Tools.Add(tool);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool created");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to create tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing tool's information.
        /// </summary>
        /// <param name="dto">The updated tool data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateTool(ToolUpdateDto dto)
        {
            try
            {
                // Retrieve existing tool
                var tool = _unitOfWork.Tools.GetById(dto.Id);
                if (tool == null)
                {
                    return ServiceResult.FailureResult("Tool not found");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Tool name is required");
                }

                tool.Name = dto.Name.Trim();
                tool.Description = dto.Description?.Trim();
                tool.IsActive = dto.IsActive;
                tool.IsUnderDevelopment = dto.IsUnderDevelopment;
                tool.UpdatedAt = DateTime.Now;

                _unitOfWork.Tools.Update(tool);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool updated");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a tool record if it has no associated requests.
        /// </summary>
        /// <param name="id">The tool ID to delete.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult DeleteTool(int id)
        {
            try
            {
                // Retrieve tool to delete
                var tool = _unitOfWork.Tools.GetById(id);
                if (tool == null)
                {
                    return ServiceResult.FailureResult("Tool not found");
                }

                // Check if tool has sync requests
                var hasRequests = _unitOfWork.AttandanceSyncRequests.Count(r => r.ToolId == id) > 0;
                if (hasRequests)
                {
                    return ServiceResult.FailureResult("Cannot delete tool with existing requests");
                }

                _unitOfWork.Tools.Remove(tool);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Tool deleted");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to delete tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the status of a tool between active and inactive.
        /// </summary>
        /// <param name="id">The tool ID to toggle.</param>
        /// <returns>Success or failure result with new status.</returns>
        public ServiceResult ToggleToolStatus(int id)
        {
            try
            {
                // Retrieve the tool
                var tool = _unitOfWork.Tools.GetById(id);
                if (tool == null)
                {
                    return ServiceResult.FailureResult("Tool not found");
                }

                tool.IsActive = !tool.IsActive;
                tool.UpdatedAt = DateTime.Now;

                _unitOfWork.Tools.Update(tool);
                _unitOfWork.SaveChanges();

                var status = tool.IsActive ? "activated" : "deactivated";
                return ServiceResult.SuccessResult($"Tool {status}");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to toggle tool status: {ex.Message}");
            }
        }
    }
}
