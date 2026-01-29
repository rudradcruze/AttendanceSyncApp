using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Helpers; // Assumed for EncryptionHelper
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing database configurations for companies.
    /// Handles CRUD operations for database connection settings,
    /// including encryption of sensitive credentials.
    /// </summary>
    public class AdminDatabaseConfigService : IAdminDatabaseConfigService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new AdminDatabaseConfigService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public AdminDatabaseConfigService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all database configurations with pagination support.
        /// Passwords are excluded from list view for security.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of database configurations.</returns>
        public ServiceResult<PagedResultDto<DatabaseConfigDto>> GetAllConfigsPaged(int page, int pageSize)
        {
            try
            {
                // Get all configurations
                var query = _unitOfWork.DatabaseConfigurations.GetAll();

                var totalRecords = query.Count();
                var configs = query.OrderByDescending(c => c.CreatedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

                // Map to DTOs with company names
                var dtos = configs.Select(c =>
                {
                    // Retrieve company name for display
                    var company = _unitOfWork.SyncCompanies.GetById(c.CompanyId);
                    return new DatabaseConfigDto
                    {
                        Id = c.Id,
                        CompanyId = c.CompanyId,
                        CompanyName = company?.Name ?? "Unknown", 
                        DatabaseIP = c.DatabaseIP,
                        DatabaseName = c.DatabaseName,
                        DatabaseUserId = c.DatabaseUserId,
                        // DatabasePassword is not sent in list for security
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    };
                }).ToList();

                return ServiceResult<PagedResultDto<DatabaseConfigDto>>.SuccessResult(new PagedResultDto<DatabaseConfigDto>
                {
                    Data = dtos,
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<DatabaseConfigDto>>.FailureResult($"Error fetching configurations: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific database configuration by its ID.
        /// Includes decrypted password for editing.
        /// </summary>
        /// <param name="id">The configuration ID.</param>
        /// <returns>Database configuration with decrypted password.</returns>
        public ServiceResult<DatabaseConfigDto> GetConfigById(int id)
        {
            try
            {
                // Retrieve the configuration
                var config = _unitOfWork.DatabaseConfigurations.GetById(id);
                if (config == null)
                {
                    return ServiceResult<DatabaseConfigDto>.FailureResult("Configuration not found");
                }

                var company = _unitOfWork.SyncCompanies.GetById(config.CompanyId);

                var dto = new DatabaseConfigDto
                {
                    Id = config.Id,
                    CompanyId = config.CompanyId,
                    CompanyName = company?.Name ?? "Unknown",
                    DatabaseIP = config.DatabaseIP,
                    DatabaseName = config.DatabaseName,
                    DatabaseUserId = config.DatabaseUserId,
                    DatabasePassword = EncryptionHelper.Decrypt(config.DatabasePassword),
                    CreatedAt = config.CreatedAt,
                    UpdatedAt = config.UpdatedAt
                };

                return ServiceResult<DatabaseConfigDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<DatabaseConfigDto>.FailureResult($"Error fetching configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the decrypted database password for a configuration.
        /// </summary>
        /// <param name="id">The configuration ID.</param>
        /// <returns>The decrypted password.</returns>
        public ServiceResult<string> GetDatabasePassword(int id)
        {
            try
            {
                // Retrieve configuration
                var config = _unitOfWork.DatabaseConfigurations.GetById(id);
                if (config == null)
                {
                    return ServiceResult<string>.FailureResult("Configuration not found");
                }

                var password = EncryptionHelper.Decrypt(config.DatabasePassword);
                return ServiceResult<string>.SuccessResult(password);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error retrieving password: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new database configuration for a company.
        /// Encrypts the password before storing.
        /// </summary>
        /// <param name="dto">The configuration data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult<string> CreateConfig(DatabaseConfigCreateDto dto)
        {
            try
            {
                // Check if configuration already exists for this company
                if (_unitOfWork.DatabaseConfigurations.HasConfiguration(dto.CompanyId))
                {
                    return ServiceResult<string>.FailureResult("Configuration already exists for this company");
                }

                // Create new configuration entity
                var config = new DatabaseConfiguration
                {
                    CompanyId = dto.CompanyId,
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseName = dto.DatabaseName,
                    DatabaseUserId = dto.DatabaseUserId,
                    // Encrypt password for security
                    DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword), 
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.DatabaseConfigurations.Add(config);
                _unitOfWork.SaveChanges();

                return ServiceResult<string>.SuccessResult("Configuration created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error creating configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing database configuration.
        /// Only encrypts and updates password if a new one is provided.
        /// </summary>
        /// <param name="dto">The updated configuration data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult<string> UpdateConfig(DatabaseConfigUpdateDto dto)
        {
            try
            {
                // Retrieve existing configuration
                var config = _unitOfWork.DatabaseConfigurations.GetById(dto.Id);
                if (config == null)
                {
                    return ServiceResult<string>.FailureResult("Configuration not found");
                }

                // Check if changing company to one that already has a config (and isn't this one)
                if (config.CompanyId != dto.CompanyId && _unitOfWork.DatabaseConfigurations.HasConfiguration(dto.CompanyId))
                {
                    return ServiceResult<string>.FailureResult("Another configuration already exists for the selected company");
                }

                config.CompanyId = dto.CompanyId;
                config.DatabaseIP = dto.DatabaseIP;
                config.DatabaseName = dto.DatabaseName;
                config.DatabaseUserId = dto.DatabaseUserId;
                
                // Only update password if provided (not null/empty)
                if (!string.IsNullOrEmpty(dto.DatabasePassword))
                {
                    config.DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword);
                }
                
                config.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseConfigurations.Update(config);
                _unitOfWork.SaveChanges();

                return ServiceResult<string>.SuccessResult("Configuration updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error updating configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a database configuration.
        /// </summary>
        /// <param name="id">The configuration ID to delete.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult<string> DeleteConfig(int id)
        {
            try
            {
                // Retrieve configuration to delete
                var config = _unitOfWork.DatabaseConfigurations.GetById(id);
                if (config == null)
                {
                    return ServiceResult<string>.FailureResult("Configuration not found");
                }

                _unitOfWork.DatabaseConfigurations.Remove(config);
                _unitOfWork.SaveChanges();

                return ServiceResult<string>.SuccessResult("Configuration deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"Error deleting configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all available companies for database configuration selection.
        /// </summary>
        /// <returns>List of all companies.</returns>
        public ServiceResult<List<CompanyDto>> GetAvailableCompanies()
        {
            try
            {
                // Get all companies for dropdown selection
                var companies = _unitOfWork.SyncCompanies.GetAll()
                    .Select(c => new CompanyDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList();

                return ServiceResult<List<CompanyDto>>.SuccessResult(companies);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<CompanyDto>>.FailureResult($"Error fetching companies: {ex.Message}");
            }
        }
    }
}