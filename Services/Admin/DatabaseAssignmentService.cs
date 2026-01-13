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

                // Check if already has configuration
                if (_unitOfWork.DatabaseConfigurations.HasConfiguration(dto.RequestId))
                {
                    return ServiceResult.FailureResult("This request already has a database configuration assigned");
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
                    RequestId = dto.RequestId,
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseUserId = dto.DatabaseUserId,
                    DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword),
                    DatabaseName = dto.DatabaseName,
                    AssignedBy = adminUserId,
                    AssignedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.DatabaseConfigurations.Add(config);

                // Update request status to Completed
                request.Status = "CP";
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
                var config = _unitOfWork.DatabaseConfigurations.GetByRequestId(requestId);
                if (config == null)
                {
                    return ServiceResult<AssignDatabaseDto>.FailureResult("No database configuration found for this request");
                }

                var dto = new AssignDatabaseDto
                {
                    RequestId = config.RequestId,
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
                var config = _unitOfWork.DatabaseConfigurations.GetByRequestId(requestId);
                if (config == null)
                {
                    return ServiceResult.FailureResult("No database configuration found for this request");
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
