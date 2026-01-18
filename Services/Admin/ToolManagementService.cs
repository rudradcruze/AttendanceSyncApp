using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class ToolManagementService : IToolManagementService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public ToolManagementService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<ToolDto>> GetToolsPaged(int page, int pageSize)
        {
            try
            {
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

        public ServiceResult<ToolDto> GetToolById(int id)
        {
            try
            {
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

        public ServiceResult CreateTool(ToolCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Tool name is required");
                }

                var tool = new Tool
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive,
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

        public ServiceResult UpdateTool(ToolUpdateDto dto)
        {
            try
            {
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

        public ServiceResult DeleteTool(int id)
        {
            try
            {
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

        public ServiceResult ToggleToolStatus(int id)
        {
            try
            {
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
