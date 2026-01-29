using System;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing database configuration assignments to company requests.
    /// Handles creation, retrieval, and updates of database configurations linked to companies.
    /// </summary>
    public class DatabaseAssignmentService : IDatabaseAssignmentService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new DatabaseAssignmentService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public DatabaseAssignmentService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Assigns a new database configuration to a company based on a sync request.
        /// Creates encrypted database credentials and links them to the company.
        /// </summary>
        /// <param name="dto">Database configuration details including IP, credentials, and database name.</param>
        /// <param name="adminUserId">The admin user ID performing the assignment.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult AssignDatabase(AssignDatabaseDto dto, int adminUserId)
        {
            try
            {
                // Validate request exists
                var request = _unitOfWork.AttandanceSyncRequests.GetById(dto.RequestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Check if company already has configuration (1:1 relationship)
                if (_unitOfWork.DatabaseConfigurations.HasConfiguration(request.CompanyId))
                {
                    return ServiceResult.FailureResult("A database configuration already exists for this company. Please manage it via 'Database Configs'.");
                }

                // Validate required fields
                if (string.IsNullOrEmpty(dto.DatabaseIP) ||
                    string.IsNullOrEmpty(dto.DatabaseUserId) ||
                    string.IsNullOrEmpty(dto.DatabasePassword) ||
                    string.IsNullOrEmpty(dto.DatabaseName))
                {
                    return ServiceResult.FailureResult("All database configuration fields are required");
                }

                // Create database configuration with encrypted password
                var config = new DatabaseConfiguration
                {
                    CompanyId = request.CompanyId,
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseUserId = dto.DatabaseUserId,
                    DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword),
                    DatabaseName = dto.DatabaseName,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.DatabaseConfigurations.Add(config);

                // Update request status to Completed
                request.IsSuccessful = true;
                request.UpdatedAt = DateTime.Now;
                _unitOfWork.AttandanceSyncRequests.Update(request);

                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database configuration assigned successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error assigning database: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the database configuration assignment for a specific request.
        /// Decrypts the password for display purposes.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <returns>Database configuration details with decrypted password.</returns>
        public ServiceResult<AssignDatabaseDto> GetAssignment(int requestId)
        {
            try
            {
                // Validate request exists
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult<AssignDatabaseDto>.FailureResult("Request not found");
                }

                // Get configuration by company ID
                var config = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (config == null)
                {
                    return ServiceResult<AssignDatabaseDto>.FailureResult("No database configuration found for this company");
                }

                // Map to DTO with decrypted password
                var dto = new AssignDatabaseDto
                {
                    RequestId = request.Id,
                    DatabaseIP = config.DatabaseIP,
                    DatabaseUserId = config.DatabaseUserId,
                    DatabasePassword = EncryptionHelper.Decrypt(config.DatabasePassword),
                    DatabaseName = config.DatabaseName
                };

                return ServiceResult<AssignDatabaseDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<AssignDatabaseDto>.FailureResult($"Error retrieving assignment: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing database configuration for a company.
        /// Re-encrypts the password if changed.
        /// </summary>
        /// <param name="requestId">The request ID associated with the company.</param>
        /// <param name="dto">Updated database configuration details.</param>
        /// <param name="adminUserId">The admin user ID performing the update.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateAssignment(int requestId, AssignDatabaseDto dto, int adminUserId)
        {
            try
            {
                // Validate request exists
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                // Get existing configuration
                var config = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (config == null)
                {
                    return ServiceResult.FailureResult("No database configuration found for this company");
                }

                // Update configuration fields
                config.DatabaseIP = dto.DatabaseIP;
                config.DatabaseUserId = dto.DatabaseUserId;
                config.DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword);
                config.DatabaseName = dto.DatabaseName;
                config.UpdatedAt = DateTime.Now;

                _unitOfWork.DatabaseConfigurations.Update(config);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Database configuration updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error updating assignment: {ex.Message}");
            }
        }
    }
}
