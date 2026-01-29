using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    /// <summary>
    /// Service for managing user accounts from an administrative perspective.
    /// Handles user retrieval, updates, and account status management operations.
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new AdminUserService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public AdminUserService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Retrieves all users with pagination support.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>Paginated list of users with their details.</returns>
        public ServiceResult<PagedResultDto<UserListDto>> GetUsersPaged(int page, int pageSize)
        {
            try
            {
                // Get total count for pagination
                var totalCount = _unitOfWork.Users.Count();

                // Retrieve paginated users and map to DTOs
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

        /// <summary>
        /// Retrieves a specific user by their ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>User details including role, status, and profile information.</returns>
        public ServiceResult<UserListDto> GetUserById(int id)
        {
            try
            {
                // Fetch user by ID
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

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="userDto">The updated user data.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult UpdateUser(UserListDto userDto)
        {
            try
            {
                // Retrieve existing user
                var user = _unitOfWork.Users.GetById(userDto.Id);
                if (user == null)
                {
                    return ServiceResult.FailureResult("User not found");
                }

                // Update user properties
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

        /// <summary>
        /// Toggles the active status of a user account (activate/deactivate).
        /// </summary>
        /// <param name="userId">The user ID to toggle.</param>
        /// <returns>Success or failure result with status message.</returns>
        public ServiceResult ToggleUserStatus(int userId)
        {
            try
            {
                // Retrieve the user
                var user = _unitOfWork.Users.GetById(userId);
                if (user == null)
                {
                    return ServiceResult.FailureResult("User not found");
                }

                // Toggle active status
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
