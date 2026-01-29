using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Models.DTOs.Sync;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Sync;

namespace AttandanceSyncApp.Services.Sync
{
    /// <summary>
    /// Service for managing attendance synchronization requests for users.
    /// Handles creation, retrieval, and cancellation of sync requests from both local and external databases.
    /// Manages database assignments and permissions for user-company relationships.
    /// </summary>
    public class SyncRequestService : ISyncRequestService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new SyncRequestService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public SyncRequestService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves user's sync requests with pagination and sorting.
        /// Fetches from external database if a company is selected and user has access,
        /// otherwise returns local database records.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="companyId">Optional company ID to filter and fetch from external DB.</param>
        /// <param name="page">Page number for pagination.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <param name="sortColumn">Column to sort by (FromDate, ToDate, Id).</param>
        /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
        /// <returns>Paginated list of sync requests with status information.</returns>
        public ServiceResult<PagedResultDto<SyncRequestDto>> GetUserRequestsPaged(int userId, int? companyId, int page, int pageSize, string sortColumn = "ToDate", string sortDirection = "DESC")
        {
            try
            {
                // If company is selected, fetch from external database
                if (companyId.HasValue)
                {
                    // Get database configuration for the selected company
                    var userDatabases = GetUserCompanyDatabases(userId);
                    if (userDatabases.Success)
                    {
                        // Find the database assignment for this company
                        var targetDb = userDatabases.Data.FirstOrDefault(d => d.CompanyId == companyId.Value);
                        if (targetDb != null)
                        {
                            var dbConfig = _unitOfWork.DatabaseConfigurations.GetById(targetDb.DatabaseConfigurationId);
                            if (dbConfig != null)
                            {
                                // Connect to external database and query attendance records
                                try
                                {
                                    using (var context = Helpers.DynamicDbHelper.CreateExternalDbContext(dbConfig))
                                    {
                                        // Query all attendance synchronizations from external database
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

                // Fallback to local database if no company selected or external fetch failed
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

        /// <summary>
        /// Creates a new synchronization request in the local database.
        /// Validates dates and ensures employee, company, and tool are active.
        /// </summary>
        /// <param name="dto">The sync request creation data.</param>
        /// <param name="userId">The user creating the request.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <returns>The created request ID on success.</returns>
        public ServiceResult<int> CreateSyncRequest(SyncRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate from date format
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

        /// <summary>
        /// Cancels a pending synchronization request.
        /// Only allows users to cancel their own pending requests.
        /// </summary>
        /// <param name="requestId">The request ID to cancel.</param>
        /// <param name="userId">The user requesting cancellation.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult CancelSyncRequest(int requestId, int userId)
        {
            try
            {
                // Retrieve the request
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

        /// <summary>
        /// Retrieves status information for multiple requests by their IDs from local database.
        /// </summary>
        /// <param name="ids">Array of request IDs.</param>
        /// <returns>List of status DTOs with ID and status text.</returns>
        public ServiceResult<IEnumerable<StatusDto>> GetStatusesByIds(int[] ids)
        {
            try
            {
                // Return empty list if no IDs provided
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

        /// <summary>
        /// Retrieves status information from external database for multiple synchronizations.
        /// Used to check real-time status of attendance syncs in company database.
        /// </summary>
        /// <param name="userId">The user ID for access validation.</param>
        /// <param name="companyId">The company ID.</param>
        /// <param name="ids">Array of external synchronization IDs.</param>
        /// <returns>List of status DTOs from external database.</returns>
        public ServiceResult<IEnumerable<StatusDto>> GetExternalStatusesByIds(int userId, int companyId, int[] ids)
        {
            try
            {
                // Return empty list if no IDs provided
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

        /// <summary>
        /// Retrieves all active users for display in UI dropdowns.
        /// </summary>
        /// <returns>List of active users with basic information.</returns>
        public ServiceResult<IEnumerable<UserDto>> GetAllUsers()
        {
            try
            {
                // Get all active users ordered by name
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

        /// <summary>
        /// Retrieves all active companies for synchronization.
        /// </summary>
        /// <returns>List of active sync companies.</returns>
        public ServiceResult<IEnumerable<SyncCompany>> GetActiveCompanies()
        {
            try
            {
                // Get companies with Active status
                var companies = _unitOfWork.SyncCompanies.GetActiveCompanies();
                return ServiceResult<IEnumerable<SyncCompany>>.SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<SyncCompany>>.FailureResult($"Failed to retrieve companies: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active tools available for synchronization.
        /// </summary>
        /// <returns>List of active tools.</returns>
        public ServiceResult<IEnumerable<Tool>> GetActiveTools()
        {
            try
            {
                // Get tools marked as active
                var tools = _unitOfWork.Tools.GetActiveTools();
                return ServiceResult<IEnumerable<Tool>>.SuccessResult(tools);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Tool>>.FailureResult($"Failed to retrieve tools: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all active employees for synchronization requests.
        /// </summary>
        /// <returns>List of active employees.</returns>
        public ServiceResult<IEnumerable<EmployeeDto>> GetActiveEmployees()
        {
            try
            {
                // Get employees marked as active
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

        /// <summary>
        /// Retrieves all company databases that a user has access to.
        /// Validates user has tool access, completed company requests, active database assignments,
        /// and non-revoked access before including in results.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of accessible company databases with configuration details.</returns>
        public ServiceResult<IEnumerable<UserCompanyDatabaseDto>> GetUserCompanyDatabases(int userId)
        {
            try
            {
                var result = new List<UserCompanyDatabaseDto>();

                // Find the attendance sync tool (with name variations)
                var validToolNames = new[] { "Attendance Sync", "Attandance Sync", "Attendance Tool", "Attandance Tool" };
                var attendanceSyncTool = _unitOfWork.Tools.GetAll()
                    .FirstOrDefault(t => validToolNames.Contains(t.Name) && t.IsActive);

                if (attendanceSyncTool == null)
                {
                    return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.SuccessResult(result);
                }

                // Check if user has this tool assigned
                var hasToolAccess = _unitOfWork.UserTools.HasActiveAssignment(userId, attendanceSyncTool.Id);
                if (!hasToolAccess)
                {
                    return ServiceResult<IEnumerable<UserCompanyDatabaseDto>>.SuccessResult(result);
                }

                // Get all non-cancelled company requests for this user and tool
                var userRequests = _unitOfWork.CompanyRequests.Find(cr =>
                    cr.UserId == userId &&
                    !cr.IsCancelled &&
                    cr.ToolId == attendanceSyncTool.Id)
                    .ToList();

                // Process each request and validate access
                foreach (var request in userRequests)
                {
                    // Check if request is completed
                    if (request.Status != "CP")
                    {
                        continue; // Skip if not completed
                    }

                    // Check if database assignment exists for this request
                    var dbAssign = _unitOfWork.DatabaseAssignments.GetByCompanyRequestId(request.Id);

                    if (dbAssign == null)
                    {
                        continue; // No assignment exists
                    }

                    // Check if assignment is revoked
                    if (dbAssign.IsRevoked)
                    {
                        continue; // Assignment is revoked
                    }

                    // Get database configuration and company details
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

        /// <summary>
        /// Creates a synchronization record directly in the external company database.
        /// This "on-the-fly" sync creates the record in real-time and returns immediately.
        /// Also creates a local tracking record with the external sync ID.
        /// </summary>
        /// <param name="dto">The sync request creation data.</param>
        /// <param name="userId">The user creating the synchronization.</param>
        /// <param name="sessionId">The current session ID.</param>
        /// <returns>The local request ID on success.</returns>
        public ServiceResult<int> CreateOnTheFlySynchronization(SyncRequestCreateDto dto, int userId, int sessionId)
        {
            try
            {
                // Validate date formats and range
                if (!DateTime.TryParse(dto.FromDate, out DateTime fromDate) || !DateTime.TryParse(dto.ToDate, out DateTime toDate))
                {
                    return ServiceResult<int>.FailureResult("Invalid Date format");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<int>.FailureResult("From Date cannot be after To Date");
                }

                // Get database configuration for user's assigned company
                var userDatabases = GetUserCompanyDatabases(userId);
                if (!userDatabases.Success)
                {
                    return ServiceResult<int>.FailureResult("Failed to retrieve database configurations");
                }

                // Find the target database for this company
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

                // Connect to external database and create sync record
                int? externalId = Helpers.DynamicDbHelper.CreateSyncInExternalDb(dbConfig, fromDate, toDate, dto.CompanyId);

                // Fail if external record creation fails
                if (externalId == null)
                {
                    return ServiceResult<int>.FailureResult("Failed to connect to company database or create record");
                }

                // Create local tracking record for this synchronization
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

        /// <summary>
        /// Converts IsSuccessful boolean to human-readable status text.
        /// </summary>
        /// <param name="isSuccessful">The success status (null = Pending, true = Completed, false = Failed).</param>
        /// <returns>Status text.</returns>
        private static string GetStatusText(bool? isSuccessful)
        {
            // Map nullable boolean to status text
            if (isSuccessful == null) return "Pending";
            return isSuccessful.Value ? "Completed" : "Failed";
        }
    }
}
