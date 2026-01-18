using System;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class DatabaseAssignmentService : IDatabaseAssignmentService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public DatabaseAssignmentService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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

                // Check if company already has configuration
                // NOTE: DatabaseConfiguration is now 1:1 with Company, not Request.
                // So we check based on the Request's CompanyId.
                if (_unitOfWork.DatabaseConfigurations.HasConfiguration(request.CompanyId))
                {
                    // If configuration exists, we might just want to return success or update it?
                    // Or maybe the user intends to OVERWRITE it?
                    // For "AssignDatabase", if it exists, let's fail and say it exists, or update it.
                    // Given the logic usually implies "creating" a link, let's assume update or fail.
                    // But wait, the previous logic was 1 request -> 1 config.
                    // Now it is 1 company -> 1 config.
                    
                    // If the company already has a config, we technically don't need to "create" a new one.
                    // But if the Admin is inputting new details, they probably want to update the Company's config.
                    
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

                // Create database configuration
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

        public ServiceResult<AssignDatabaseDto> GetAssignment(int requestId)
        {
            try
            {
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult<AssignDatabaseDto>.FailureResult("Request not found");
                }

                var config = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (config == null)
                {
                    return ServiceResult<AssignDatabaseDto>.FailureResult("No database configuration found for this company");
                }

                var dto = new AssignDatabaseDto
                {
                    RequestId = request.Id, // Kept for compatibility, though config is company-bound
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

        public ServiceResult UpdateAssignment(int requestId, AssignDatabaseDto dto, int adminUserId)
        {
            try
            {
                var request = _unitOfWork.AttandanceSyncRequests.GetById(requestId);
                if (request == null)
                {
                    return ServiceResult.FailureResult("Request not found");
                }

                var config = _unitOfWork.DatabaseConfigurations.GetByCompanyId(request.CompanyId);
                if (config == null)
                {
                    return ServiceResult.FailureResult("No database configuration found for this company");
                }

                // Update configuration
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