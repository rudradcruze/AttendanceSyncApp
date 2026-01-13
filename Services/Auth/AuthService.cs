using System;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ISessionService _sessionService;

        public AuthService(
            IAuthUnitOfWork unitOfWork,
            IGoogleAuthService googleAuthService,
            ISessionService sessionService)
        {
            _unitOfWork = unitOfWork;
            _googleAuthService = googleAuthService;
            _sessionService = sessionService;
        }

        public ServiceResult<UserDto> LoginWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                // Validate Google token
                var googleResult = _googleAuthService.ValidateIdToken(googleAuth.IdToken);
                if (!googleResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(googleResult.Message);
                }

                var googleUser = googleResult.Data;

                // Find or create user
                var user = _unitOfWork.Users.GetByGoogleId(googleUser.GoogleId)
                        ?? _unitOfWork.Users.GetByEmail(googleUser.Email);

                if (user == null)
                {
                    // Auto-create user on first Google sign-in
                    user = new User
                    {
                        Name = googleUser.Name,
                        Email = googleUser.Email,
                        GoogleId = googleUser.GoogleId,
                        ProfilePicture = googleUser.Picture,
                        Role = "USER",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _unitOfWork.Users.Add(user);
                    _unitOfWork.SaveChanges();
                }
                else if (string.IsNullOrEmpty(user.GoogleId))
                {
                    // Link Google account to existing email user
                    user.GoogleId = googleUser.GoogleId;
                    user.ProfilePicture = googleUser.Picture;
                    user.UpdatedAt = DateTime.Now;
                    _unitOfWork.Users.Update(user);
                    _unitOfWork.SaveChanges();
                }

                if (!user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("Account is deactivated");
                }

                // Create session
                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);
                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);
                return ServiceResult<UserDto>.SuccessResult(userDto, "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public ServiceResult<UserDto> LoginAdmin(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return ServiceResult<UserDto>.FailureResult("Email and password are required");
                }

                var user = _unitOfWork.Users.GetByEmail(email);

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                if (user.Role != "ADMIN")
                {
                    return ServiceResult<UserDto>.FailureResult("Admin access required");
                }

                if (!user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("Account is deactivated");
                }

                // Verify password
                if (!EncryptionHelper.VerifyPassword(password, user.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                // Create session
                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);
                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);
                return ServiceResult<UserDto>.SuccessResult(userDto, "Admin login successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Login failed: {ex.Message}");
            }
        }

        public ServiceResult<UserDto> RegisterWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                // Validate Google token
                var googleResult = _googleAuthService.ValidateIdToken(googleAuth.IdToken);
                if (!googleResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(googleResult.Message);
                }

                var googleUser = googleResult.Data;

                // Check if user already exists
                if (_unitOfWork.Users.EmailExists(googleUser.Email))
                {
                    return ServiceResult<UserDto>.FailureResult("Account already exists. Please sign in.");
                }

                // Create new user
                var user = new User
                {
                    Name = googleUser.Name,
                    Email = googleUser.Email,
                    GoogleId = googleUser.GoogleId,
                    ProfilePicture = googleUser.Picture,
                    Role = "USER",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Users.Add(user);
                _unitOfWork.SaveChanges();

                // Create session
                var sessionResult = _sessionService.CreateSession(user.Id, sessionInfo);
                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var userDto = MapToUserDto(user, sessionResult.Data.SessionToken);
                return ServiceResult<UserDto>.SuccessResult(userDto, "Registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Registration failed: {ex.Message}");
            }
        }

        public ServiceResult Logout(string sessionToken)
        {
            return _sessionService.EndSession(sessionToken);
        }

        public ServiceResult<UserDto> GetCurrentUser(string sessionToken)
        {
            try
            {
                var sessionResult = _sessionService.GetActiveSession(sessionToken);
                if (!sessionResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(sessionResult.Message);
                }

                var user = _unitOfWork.Users.GetById(sessionResult.Data.UserId);
                if (user == null || !user.IsActive)
                {
                    return ServiceResult<UserDto>.FailureResult("User not found or inactive");
                }

                var userDto = MapToUserDto(user, sessionToken);
                return ServiceResult<UserDto>.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Error: {ex.Message}");
            }
        }

        public bool ValidateSession(string sessionToken)
        {
            var result = _sessionService.GetActiveSession(sessionToken);
            return result.Success;
        }

        private UserDto MapToUserDto(User user, string sessionToken)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ProfilePicture = user.ProfilePicture,
                SessionToken = sessionToken,
                IsActive = user.IsActive
            };
        }
    }
}
