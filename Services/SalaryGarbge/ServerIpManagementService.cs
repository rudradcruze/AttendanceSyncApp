using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Services.SalaryGarbge
{
    /// <summary>
    /// Service responsible for managing Server IP configurations.
    /// Handles CRUD operations, activation control, secure credential handling,
    /// and initial database access population.
    /// </summary>
    public class ServerIpManagementService : IServerIpManagementService
    {
        /// Unit of work for accessing repositories.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of ServerIpManagementService.
        /// </summary>
        /// <param name="unitOfWork">Authentication unit of work.</param>
        public ServerIpManagementService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves server IPs in a paginated format.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Records per page.</param>
        /// <returns>Paged list of server IPs.</returns>
        public ServiceResult<PagedResultDto<ServerIpDto>>
            GetServerIpsPaged(int page, int pageSize)
        {
            try
            {
                // Retrieve total record count
                var totalCount = _unitOfWork.ServerIps.Count();

                // Fetch paginated server IPs
                var serverIps = _unitOfWork.ServerIps.GetAll()
                    .OrderByDescending(s => s.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new ServerIpDto
                    {
                        Id = s.Id,
                        IpAddress = s.IpAddress,
                        DatabaseUser = s.DatabaseUser,
                        DatabasePassword = null, // Never expose passwords in lists
                        Description = s.Description,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<ServerIpDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = serverIps
                };

                return ServiceResult<PagedResultDto<ServerIpDto>>
                    .SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<ServerIpDto>>
                    .FailureResult($"Failed to retrieve server IPs: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a single server IP by its identifier.
        /// </summary>
        /// <param name="id">Server IP ID.</param>
        /// <returns>Server IP details.</returns>
        public ServiceResult<ServerIpDto> GetServerIpById(int id)
        {
            try
            {
                // Fetch server IP entity
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult<ServerIpDto>
                        .FailureResult("Server IP not found");
                }

                // Attempt to decrypt password for admin view
                string decryptedPassword;
                try
                {
                    decryptedPassword =
                        EncryptionHelper.Decrypt(serverIp.DatabasePassword);
                }
                catch
                {
                    decryptedPassword = "[Decryption failed]";
                }

                var dto = new ServerIpDto
                {
                    Id = serverIp.Id,
                    IpAddress = serverIp.IpAddress,
                    DatabaseUser = serverIp.DatabaseUser,
                    DatabasePassword = decryptedPassword,
                    Description = serverIp.Description,
                    IsActive = serverIp.IsActive,
                    CreatedAt = serverIp.CreatedAt,
                    UpdatedAt = serverIp.UpdatedAt
                };

                return ServiceResult<ServerIpDto>
                    .SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ServerIpDto>
                    .FailureResult($"Failed to retrieve server IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new server IP configuration.
        /// Automatically populates database access entries.
        /// </summary>
        /// <param name="dto">Server IP creation data.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult CreateServerIp(ServerIpCreateDto dto)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.IpAddress))
                    return ServiceResult.FailureResult("Server IP address is required");

                if (string.IsNullOrWhiteSpace(dto.DatabaseUser))
                    return ServiceResult.FailureResult("Database user is required");

                if (string.IsNullOrWhiteSpace(dto.DatabasePassword))
                    return ServiceResult.FailureResult("Database password is required");

                // Prevent duplicate IP addresses
                if (_unitOfWork.ServerIps
                    .IpAddressExists(dto.IpAddress.Trim()))
                {
                    return ServiceResult
                        .FailureResult("Server IP address already exists");
                }

                // Encrypt database password before persistence
                var encryptedPassword =
                    EncryptionHelper.Encrypt(dto.DatabasePassword);

                var serverIp = new ServerIp
                {
                    IpAddress = dto.IpAddress.Trim(),
                    DatabaseUser = dto.DatabaseUser.Trim(),
                    DatabasePassword = encryptedPassword,
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.ServerIps.Add(serverIp);
                _unitOfWork.SaveChanges();

                // Auto-populate DatabaseAccess entries
                try
                {
                    var databases = GetDatabasesFromNewServer(
                        serverIp.IpAddress,
                        serverIp.DatabaseUser,
                        serverIp.DatabasePassword);

                    foreach (var dbName in databases)
                    {
                        _unitOfWork.DatabaseAccess.Add(
                            new DatabaseAccess
                            {
                                ServerIpId = serverIp.Id,
                                DatabaseName = dbName,
                                HasAccess = true,
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            });
                    }

                    _unitOfWork.SaveChanges();
                }
                catch
                {
                    // Failure here should not block server creation
                    // Admin can manage access manually later
                }

                return ServiceResult
                    .SuccessResult("Server IP created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to create server IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing server IP configuration.
        /// </summary>
        /// <param name="dto">Server IP update data.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult UpdateServerIp(ServerIpUpdateDto dto)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(dto.Id);
                if (serverIp == null)
                {
                    return ServiceResult
                        .FailureResult("Server IP not found");
                }

                if (string.IsNullOrWhiteSpace(dto.IpAddress))
                    return ServiceResult.FailureResult("Server IP address is required");

                if (string.IsNullOrWhiteSpace(dto.DatabaseUser))
                    return ServiceResult.FailureResult("Database user is required");

                // Ensure IP uniqueness
                if (_unitOfWork.ServerIps
                    .IpAddressExists(dto.IpAddress.Trim(), dto.Id))
                {
                    return ServiceResult
                        .FailureResult("Server IP address already exists");
                }

                serverIp.IpAddress = dto.IpAddress.Trim();
                serverIp.DatabaseUser = dto.DatabaseUser.Trim();

                // Update password only if provided
                if (!string.IsNullOrWhiteSpace(dto.DatabasePassword))
                {
                    serverIp.DatabasePassword =
                        EncryptionHelper.Encrypt(dto.DatabasePassword);
                }

                serverIp.Description = dto.Description?.Trim();
                serverIp.IsActive = dto.IsActive;
                serverIp.UpdatedAt = DateTime.Now;

                _unitOfWork.ServerIps.Update(serverIp);
                _unitOfWork.SaveChanges();

                return ServiceResult
                    .SuccessResult("Server IP updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to update server IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Permanently deletes a server IP configuration.
        /// </summary>
        /// <param name="id">Server IP ID.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult DeleteServerIp(int id)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult
                        .FailureResult("Server IP not found");
                }

                _unitOfWork.ServerIps.Remove(serverIp);
                _unitOfWork.SaveChanges();

                return ServiceResult
                    .SuccessResult("Server IP deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to delete server IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the active status of a server IP.
        /// </summary>
        /// <param name="id">Server IP ID.</param>
        /// <returns>Operation result.</returns>
        public ServiceResult ToggleServerIpStatus(int id)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult
                        .FailureResult("Server IP not found");
                }

                serverIp.IsActive = !serverIp.IsActive;
                serverIp.UpdatedAt = DateTime.Now;

                _unitOfWork.ServerIps.Update(serverIp);
                _unitOfWork.SaveChanges();

                var status = serverIp.IsActive
                    ? "activated"
                    : "deactivated";

                return ServiceResult
                    .SuccessResult($"Server IP {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to toggle server IP status: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all user databases from a newly added server.
        /// </summary>
        private List<string> GetDatabasesFromNewServer(
            string serverIp,
            string userId,
            string encryptedPassword)
        {
            var databases = new List<string>();

            // Decrypt password for SQL connection
            var decryptedPassword =
                EncryptionHelper.Decrypt(encryptedPassword);

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverIp,
                InitialCatalog = "master",
                UserID = userId,
                Password = decryptedPassword,
                IntegratedSecurity = false,
                ConnectTimeout = 30,
                Encrypt = false,
                TrustServerCertificate = true
            };

            using (var connection =
                new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                // Retrieve non-system databases
                var query = @"
                    SELECT name FROM sys.databases
                    WHERE state_desc = 'ONLINE'
                      AND name NOT IN ('master', 'tempdb', 'model', 'msdb')
                    ORDER BY name";

                using (var command =
                    new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }
            }

            return databases;
        }
    }
}
