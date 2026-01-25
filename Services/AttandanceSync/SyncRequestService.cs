using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Models.DTOs.AttandanceSync;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.AttandanceSync;

namespace AttandanceSyncApp.Services.AttandanceSync
{
    public class SyncRequestService : ISyncRequestService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public SyncRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<SyncRequestDto>> GetUserRequestsPaged(int userId, int? companyId, int page, int pageSize, string sortColumn = "ToDate", string sortDirection = "DESC")
        {
            try
            {
                // If a specific company is selected, try to fetch from external DB
                if (companyId.HasValue)
                {
                    // 1. Get the DB Config for this company
                    var userDatabases = GetUserCompanyDatabases(userId);
                    if (userDatabases.Success)
                    {
                        var targetDb = userDatabases.Data.FirstOrDefault(d => d.CompanyId == companyId.Value);
                        if (targetDb != null)
                        {
                            var dbConfig = _unitOfWork.DatabaseConfigurations.GetById(targetDb.DatabaseConfigurationId);
                            if (dbConfig != null)
                            {
                                // 2. Connect and Query External DB
                                try
                                {
                                    using (var context = Helpers.DynamicDbHelper.CreateExternalDbContext(dbConfig))
                                    {
                                        // Get ALL AttandanceSynchronizations (no CompanyId filter)
                                        var baseQuery = context.AttandanceSynchronizations.AsQueryable();

                                        // Apply sorting
                                        bool isDescending = string.Equals(sortDirection, "DESC", StringComparison.OrdinalIgnoreCase);
                                        switch (sortColumn?.ToLower())
                                        {
                                            case "fromdate":
                                                baseQuery = isDescending
                                                    ? baseQuery.OrderByDescending(s => s.FromDate)
                                                    : baseQuery.OrderBy(s => s.FromDate);
                                                break;
                                            case "todate":
                                                baseQuery = isDescending
                                                    ? baseQuery.OrderByDescending(s => s.ToDate)
                                                    : baseQuery.OrderBy(s => s.ToDate);
                                                break;
                                            case "id":
                                            default:
                                                baseQuery = isDescending
                                                    ? baseQuery.OrderByDescending(s => s.Id)
                                                    : baseQuery.OrderBy(s => s.Id);
                                                break;
                                        }

                                        var totalExternal = baseQuery.Count();
                                        var attendanceRecords = baseQuery
                                            .Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToList();

                                        // Get all company IDs from the attendance records
                                        var companyIds = attendanceRecords.Select(a => a.CompanyId).Distinct().ToList();

                                        // Fetch companies from external DB to get company names
                                        var companies = context.Companies
                                            .Where(c => companyIds.Contains(c.Id))
                                            .ToDictionary(c => c.Id, c => c.CompanyName);

                                        // Map to DTOs with company names
                                        var externalRequests = attendanceRecords.Select(r => new SyncRequestDto
                                        {
                                            Id = r.Id,
                                            UserName = "External",
                                            EmployeeName = targetDb.EmployeeName,
                                            CompanyName = companies.ContainsKey(r.CompanyId) ? companies[r.CompanyId] : "N/A",
                                            ToolName = targetDb.ToolName,
                                            ExternalSyncId = r.Id,
                                            IsSuccessful = r.Status == "CP",
                                            Status = r.Status,
                                            FromDate = r.FromDate,
                                            ToDate = r.ToDate,
                                            CreatedAt = DateTime.Now
                                        }).ToList();

                                        return ServiceResult<PagedResultDto<SyncRequestDto>>.SuccessResult(new PagedResultDto<SyncRequestDto>
                                        {
                                            TotalRecords = totalExternal,
                                            Page = page,
                                            PageSize = pageSize,
                                            Data = externalRequests
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle connection error, fallback to local or return error
                                    return ServiceResult<PagedResultDto<SyncRequestDto>>.FailureResult($"Failed to connect to company database: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                // Fallback to Local DB (Original Logic)
                var totalCount = _unitOfWork.AttandanceSyncRequests.GetTotalCountByUserId(userId);
                var requests = _unitOfWork.AttandanceSyncRequests.GetPagedByUserId(userId, page, pageSize)
                    .ToList()
                    .Select(r => new SyncRequestDto
                    {
                        Id = r.Id,
                        UserName = r.User?.Name ?? "Unknown",
                        EmployeeName = r.Employee?.Name ?? "Unknown",
                        CompanyName = r.Company?.Name ?? "Unknown",
                        ToolName = r.Tool?.Name ?? "Unknown",
                        ExternalSyncId = r.ExternalSyncId,
                        IsSuccessful = r.IsSuccessful,
                        Status = GetStatusText(r.IsSuccessful),
                        FromDate = r.FromDate,
                        ToDate = r.ToDate,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<SyncRequestDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = requests
                };

                return ServiceResult<PagedResultDto<SyncRequestDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<SyncRequestDto>>.FailureResult($"Failed to retrieve requests: {ex.Message}");
            }
        }

        public ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate dates
                if (!DateTime.TryParse(dto.FromDate, out DateTime fromDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid From Date format");
                }

                if (!DateTime.TryParse(dto.ToDate, out DateTime toDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid To Date format");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<int>.FailureResult("From Date cannot be after To Date");
                }

                // Validate employee exists
                var employee = _unitOfWork.Employees.GetById(dto.EmployeeId);
                if (employee == null || !employee.IsActive)
                {
                    return ServiceResult<int>.FailureResult("Selected employee not found or inactive");
                }

                // Validate company exists
                var company = _unitOfWork.SyncCompanies.GetById(dto.CompanyId);
                if (company == null || company.Status != "Active")
                {
                    return ServiceResult<int>.FailureResult("Selected company not found or inactive");
                }

                // Validate tool exists
                var tool = _unitOfWork.Tools.GetById(dto.ToolId);
                if (tool == null || !tool.IsActive)
                {
                    return ServiceResult<int>.FailureResult("Selected tool not found or inactive");
                }

                // Create sync request
                var request = new AttandanceSyncRequest
                {
                    UserId = userId,
                    EmployeeId = dto.EmployeeId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId,
                    SessionId = sessionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    IsSuccessful = null, // Pending
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.AttandanceSyncRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(request.Id, "Request created");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Failed to create request: {ex.Message}");
            }
        }

        public ServiceResult CancelSyncRequest(int requestId, int userId)
        {
            try
            {
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Only allow cancellation of own requests
                if (request.UserId != userId)
                {
                    return ServiceResult.FailureResult("Not authorized");
                }

                // Only allow cancellation of pending requests (IsSuccessful == null)
                if (request.IsSuccessful != null)
                {
                    return ServiceResult.FailureResult("Cannot cancel processed request");
                }

                // Mark as cancelled (IsSuccessful = false)
                request.IsSuccessful = false;
                request.UpdatedAt = DateTime.Now;

                _unitOfWork.AttandanceSyncRequests.Update(request);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Request cancelled");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to cancel request: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(new List<StatusDto>());
                }

                // Materialize the data first, then apply the GetStatusText method
                var requests = _unitOfWork.AttandanceSyncRequests.Find(r => ids.Contains(r.Id))
                    .ToList()
                    .Select(r => new StatusDto
                    {
                        Id = r.Id,
                        Status = GetStatusText(r.IsSuccessful)
                    })
                    .ToList();

                return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(requests);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<StatusDto>>.FailureResult($"Failed to retrieve statuses: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<StatusDto>> GetExternalStatusesByIds(int userId, int companyId, int[] ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(new List<StatusDto>());
                }

                // Get the DB Config for this company
                var userDatabases = GetUserCompanyDatabases(userId);
                if (!userDatabases.Success)
                {
                    return ServiceResult<IEnumerable<StatusDto>>.FailureResult("Failed to retrieve database configurations");
                }

                var targetDb = userDatabases.Data.FirstOrDefault(d => d.CompanyId == companyId);
                if (targetDb == null)
                {
                    return ServiceResult<IEnumerable<StatusDto>>.FailureResult("No database assignment found for this company");
                }

                var dbConfig = _unitOfWork.DatabaseConfigurations.GetById(targetDb.DatabaseConfigurationId);
                if (dbConfig == null)
                {
                    return ServiceResult<IEnumerable<StatusDto>>.FailureResult("Database configuration not found");
                }

                // Query external database for statuses
                using (var context = Helpers.DynamicDbHelper.CreateExternalDbContext(dbConfig))
                {
                    var statuses = context.AttandanceSynchronizations
                        .Where(s => ids.Contains(s.Id))
                        .Select(s => new StatusDto
                        {
                            Id = s.Id,
                            Status = s.Status
                        })
                        .ToList();

                    return ServiceResult<IEnumerable<StatusDto>>.SuccessResult(statuses);
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<StatusDto>>.FailureResult($"Failed to retrieve statuses: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<UserDto>> GetAllUsers()
        {
            try
            {
                var users = _unitOfWork.Users.GetAll()
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .ToList();

                return ServiceResult<IEnumerable<UserDto>>.SuccessResult(users);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UserDto>>.FailureResult($"Failed to retrieve users: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies()
        {
            try
            {
                var companies = _unitOfWork.SyncCompanies.GetActiveCompanies();
                return ServiceResult<IEnumerable<SyncCompany>>.SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<SyncCompany>>.FailureResult($"Failed to retrieve companies: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<Tool>> GetActiveTools()
        {
            try
            {
                var tools = _unitOfWork.Tools.GetActiveTools();
                return ServiceResult<IEnumerable<Tool>>.SuccessResult(tools);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Tool>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees()
        {
            try
            {
                var employees = _unitOfWork.Employees.GetActiveEmployees()
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        IsActive = e.IsActive
                    })
                    .ToList();

                return ServiceResult<IEnumerable<EmployeeDto>>.SuccessResult(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<EmployeeDto>>.FailureResult($"Failed to retrieve employees: {ex.Message}");
            }
        }

        public ServiceResult<IEnumerable<UserCompanyDatabaseDto>> GetUserCompanyDatabases(int userId)
        {
            try
            {
                var result = new List<UserCompanyDatabaseDto>();

                // 1. Find the "Attendance Sync" tool (or variations)
                var validToolNames = new[] { "Attendance Sync", "Attandance Sync", "Attendance Tool", "Attandance Tool" };
                var attendanceSyncTool = _unitOfWork.Tools.GetAll()
                    .FirstOrDefault(t => validToolNames.Contains(t.Name) && t.IsActive);

                if (attendanceSyncTool == null)
                {
                    return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.SuccessResult(result);
                }

                // 2. Check if user has this tool assigned explicitly
                var hasToolAccess = _unitOfWork.UserTools.HasActiveAssignment(userId, attendanceSyncTool.Id);
                if (!hasToolAccess)
                {
                    return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.SuccessResult(result);
                }

                // 3. Get ALL company requests for this user and tool that are NOT cancelled
                // We fetch all non-cancelled ones first to then explicitly check status as requested
                var userRequests = _unitOfWork.CompanyRequests.Find(cr =>
                    cr.UserId == userId &&
                    !cr.IsCancelled &&
                    cr.ToolId == attendanceSyncTool.Id)
                    .ToList();

                foreach (var request in userRequests)
                {
                    // 4. EXPLICIT CHECK: Is the status "Completed" (CP)?
                    // As requested: Check the company request status.
                    if (request.Status != "CP")
                    {
                        continue; // Skip if not completed
                    }

                    // 5. EXPLICIT CHECK: Does a database assignment exist for this request?
                    // As requested: Using that request id go to DatabaseAssignments then check that it exist or not.
                    var dbAssign = _unitOfWork.DatabaseAssignments.GetByCompanyRequestId(request.Id);

                    if (dbAssign == null)
                    {
                        // Assignment does not exist
                        continue;
                    }

                    // 6. EXPLICIT CHECK: Is the assignment revoked?
                    // As requested: If exist then it is revoked or not.
                    if (dbAssign.IsRevoked)
                    {
                        // Assignment exists but is revoked
                        continue;
                    }

                    // 7. Get Configuration and Company details if valid
                    var dbConfig = _unitOfWork.DatabaseConfigurations.GetById(dbAssign.DatabaseConfigurationId);
                    var company = _unitOfWork.SyncCompanies.GetById(request.CompanyId);

                    if (dbConfig != null && company != null)
                    {
                        result.Add(new UserCompanyDatabaseDto
                        {
                            CompanyRequestId = request.Id,
                            DatabaseAssignmentId = dbAssign.Id,
                            CompanyId = company.Id,
                            CompanyName = company.Name,
                            DatabaseName = dbConfig.DatabaseName,
                            DatabaseConfigurationId = dbConfig.Id,
                            ToolId = attendanceSyncTool.Id,
                            ToolName = attendanceSyncTool.Name,
                            EmployeeId = request.EmployeeId,
                            EmployeeName = request.Employee?.Name ?? "Unknown"
                        });
                    }
                }

                return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.FailureResult($"Failed to retrieve company databases: {ex.Message}");
            }
        }

        public ServiceResult<int> CreateOnTheFlySynchronization(SyncRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // 1. Validate dates
                if (!DateTime.TryParse(dto.FromDate, out DateTime fromDate) || !DateTime.TryParse(dto.ToDate, out DateTime toDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid Date format");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<int>.FailureResult("From Date cannot be after To Date");
                }

                // 2. Get Database Configuration using the explicit logic we built earlier
                // We need to find the valid assignment for this user/company/tool
                var userDatabases = GetUserCompanyDatabases(userId);
                if (!userDatabases.Success)
                {
                    return ServiceResult<int>.FailureResult("Failed to retrieve database configurations");
                }

                var targetDb = userDatabases.Data.FirstOrDefault(d => d.CompanyId == dto.CompanyId);
                if (targetDb == null)
                {
                    return ServiceResult<int>.FailureResult("No active database assignment found for this company");
                }

                var dbConfig = _unitOfWork.DatabaseConfigurations.GetById(targetDb.DatabaseConfigurationId);
                if (dbConfig == null)
                {
                    return ServiceResult<int>.FailureResult("Database configuration not found");
                }

                // 3. Connect to External DB and Create Record
                int? externalId = Helpers.DynamicDbHelper.CreateSyncInExternalDb(dbConfig, fromDate, toDate, dto.CompanyId);

                // If external creation fails, we might still want to record the attempt locally but mark as failed?
                // Or fail completely? User said "it will connect... and show below view".
                // Let's assume if it fails to connect, the request fails.
                if (externalId == null)
                {
                    return ServiceResult<int>.FailureResult("Failed to connect to company database or create record");
                }

                // 4. Create Local Request Record
                var request = new AttandanceSyncRequest
                {
                    UserId = userId,
                    EmployeeId = dto.EmployeeId,
                    CompanyId = dto.CompanyId,
                    ToolId = dto.ToolId, // Should match the tool ID passed in DTO
                    SessionId = sessionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ExternalSyncId = externalId,
                    IsSuccessful = true, // It was successfully created in external DB
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.AttandanceSyncRequests.Add(request);
                _unitOfWork.SaveChanges();

                return ServiceResult<int>.SuccessResult(request.Id, "Synchronization created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult($"Error: {ex.Message}");
            }
        }

        private static string GetStatusText(bool? isSuccessful)
        {
            if (isSuccessful == null) return "Pending";
            return isSuccessful.Value ? "Completed" : "Failed";
        }
    }
}
