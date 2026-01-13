using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public AdminUserService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<PagedResultDto<UserListDto>> GetUsersPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.Users.Count();
                var users = _unitOfWork.Users.GetAll()
                    .OrderByDescending(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserListDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        ProfilePicture = u.ProfilePicture,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResultDto<UserListDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = users
                };

                return ServiceResult<PagedResultDto<UserListDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<UserListDto>>.FailureResult($"Error retrieving users: {ex.Message}");
            }
        }

        public ServiceResult<UserListDto> GetUserById(int id)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(id);
                if (user == null)
                {
                    return ServiceResult<UserListDto>.FailureResult("User not found");
                }

                var userDto = new UserListDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    ProfilePicture = user.ProfilePicture,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return ServiceResult<UserListDto>.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserListDto>.FailureResult($"Error retrieving user: {ex.Message}");
            }
        }

        public ServiceResult UpdateUser(UserListDto userDto)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(userDto.Id);
                if (user == null)
                {
                    return ServiceResult.FailureResult("User not found");
                }

                user.Name = userDto.Name;
                user.Email = userDto.Email;
                user.IsActive = userDto.IsActive;
                user.UpdatedAt = DateTime.Now;

                _unitOfWork.Users.Update(user);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("User updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error updating user: {ex.Message}");
            }
        }

        public ServiceResult ToggleUserStatus(int userId)
        {
            try
            {
                var user = _unitOfWork.Users.GetById(userId);
                if (user == null)
                {
                    return ServiceResult.FailureResult("User not found");
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.Now;

                _unitOfWork.Users.Update(user);
                _unitOfWork.SaveChanges();

                var status = user.IsActive ? "activated" : "deactivated";
                return ServiceResult.SuccessResult($"User {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error toggling user status: {ex.Message}");
            }
        }
    }
}
