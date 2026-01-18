using System;
using System.Collections.Generic;
using System.Linq;
using AttandanceSyncApp.Helpers; // Assumed for EncryptionHelper
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.Sync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class AdminDatabaseConfigService : IAdminDatabaseConfigService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminDatabaseConfigService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<DatabaseConfigDto>> GetAllConfigsPaged(int page, int pageSize)
        {
            try
            {
                var query = _unitOfWork.DatabaseConfigurations.GetAll();

                var totalRecords = query.Count();
                var configs = query.OrderByDescending(c => c.CreatedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

                var dtos = configs.Select(c =>
                {
                    // SyncCompany uses 'Name' not 'CompanyName'
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

        public ServiceResult<DatabaseConfigDto> GetConfigById(int id)
        {
            try
            {
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

        public ServiceResult<string> GetDatabasePassword(int id)
        {
            try
            {
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

        public ServiceResult<string> CreateConfig(DatabaseConfigCreateDto dto)
        {
            try
            {
                if (_unitOfWork.DatabaseConfigurations.HasConfiguration(dto.CompanyId))
                {
                    return ServiceResult<string>.FailureResult("Configuration already exists for this company");
                }

                var config = new DatabaseConfiguration
                {
                    CompanyId = dto.CompanyId,
                    DatabaseIP = dto.DatabaseIP,
                    DatabaseName = dto.DatabaseName,
                    DatabaseUserId = dto.DatabaseUserId,
                    // Encrypt password if EncryptionHelper exists, otherwise store as is (or handle as per project security)
                    // Assuming EncryptionHelper is available as it was used in DatabaseAssignmentService
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

        public ServiceResult<string> UpdateConfig(DatabaseConfigUpdateDto dto)
        {
            try
            {
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

        public ServiceResult<string> DeleteConfig(int id)
        {
            try
            {
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

        public ServiceResult<List<CompanyDto>> GetAvailableCompanies()
        {
            try
            {
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