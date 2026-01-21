using System;
using System.Linq;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.SalaryGarbge;
using AttandanceSyncApp.Models.SalaryGarbge;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.SalaryGarbge;

namespace AttandanceSyncApp.Services.SalaryGarbge
{
    public class ServerIpManagementService : IServerIpManagementService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public ServerIpManagementService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<ServerIpDto>> GetServerIpsPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.ServerIps.Count();
                var serverIps = _unitOfWork.ServerIps.GetAll()
                    .OrderByDescending(s => s.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new ServerIpDto
                    {
                        Id = s.Id,
                        IpAddress = s.IpAddress,
                        DatabaseUser = s.DatabaseUser,
                        DatabasePassword = null, // Don't expose password in list
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

                return ServiceResult<PagedResultDto<ServerIpDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<ServerIpDto>>.FailureResult($"Failed to retrieve server IPs: {ex.Message}");
            }
        }

        public ServiceResult<ServerIpDto> GetServerIpById(int id)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult<ServerIpDto>.FailureResult("Server IP not found");
                }

                // Decrypt the password for display
                string decryptedPassword = null;
                try
                {
                    decryptedPassword = EncryptionHelper.Decrypt(serverIp.DatabasePassword);
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

                return ServiceResult<ServerIpDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ServerIpDto>.FailureResult($"Failed to retrieve server IP: {ex.Message}");
            }
        }

        public ServiceResult CreateServerIp(ServerIpCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.IpAddress))
                {
                    return ServiceResult.FailureResult("Server IP address is required");
                }

                if (string.IsNullOrWhiteSpace(dto.DatabaseUser))
                {
                    return ServiceResult.FailureResult("Database user is required");
                }

                if (string.IsNullOrWhiteSpace(dto.DatabasePassword))
                {
                    return ServiceResult.FailureResult("Database password is required");
                }

                // Check if IP address already exists
                if (_unitOfWork.ServerIps.IpAddressExists(dto.IpAddress.Trim()))
                {
                    return ServiceResult.FailureResult("Server IP address already exists");
                }

                // Encrypt the password before saving
                var encryptedPassword = EncryptionHelper.Encrypt(dto.DatabasePassword);

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

                return ServiceResult.SuccessResult("Server IP created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to create server IP: {ex.Message}");
            }
        }

        public ServiceResult UpdateServerIp(ServerIpUpdateDto dto)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(dto.Id);
                if (serverIp == null)
                {
                    return ServiceResult.FailureResult("Server IP not found");
                }

                if (string.IsNullOrWhiteSpace(dto.IpAddress))
                {
                    return ServiceResult.FailureResult("Server IP address is required");
                }

                if (string.IsNullOrWhiteSpace(dto.DatabaseUser))
                {
                    return ServiceResult.FailureResult("Database user is required");
                }

                // Check if IP address already exists (excluding current record)
                if (_unitOfWork.ServerIps.IpAddressExists(dto.IpAddress.Trim(), dto.Id))
                {
                    return ServiceResult.FailureResult("Server IP address already exists");
                }

                serverIp.IpAddress = dto.IpAddress.Trim();
                serverIp.DatabaseUser = dto.DatabaseUser.Trim();

                // Only update password if a new one is provided
                if (!string.IsNullOrWhiteSpace(dto.DatabasePassword))
                {
                    serverIp.DatabasePassword = EncryptionHelper.Encrypt(dto.DatabasePassword);
                }

                serverIp.Description = dto.Description?.Trim();
                serverIp.IsActive = dto.IsActive;
                serverIp.UpdatedAt = DateTime.Now;

                _unitOfWork.ServerIps.Update(serverIp);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Server IP updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to update server IP: {ex.Message}");
            }
        }

        public ServiceResult DeleteServerIp(int id)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult.FailureResult("Server IP not found");
                }

                _unitOfWork.ServerIps.Remove(serverIp);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Server IP deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to delete server IP: {ex.Message}");
            }
        }

        public ServiceResult ToggleServerIpStatus(int id)
        {
            try
            {
                var serverIp = _unitOfWork.ServerIps.GetById(id);
                if (serverIp == null)
                {
                    return ServiceResult.FailureResult("Server IP not found");
                }

                serverIp.IsActive = !serverIp.IsActive;
                serverIp.UpdatedAt = DateTime.Now;

                _unitOfWork.ServerIps.Update(serverIp);
                _unitOfWork.SaveChanges();

                var status = serverIp.IsActive ? "activated" : "deactivated";
                return ServiceResult.SuccessResult($"Server IP {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Failed to toggle server IP status: {ex.Message}");
            }
        }
    }
}
