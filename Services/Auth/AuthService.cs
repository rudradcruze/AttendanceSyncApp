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

        public ServiceResult<UserDto> LoginUser(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return ServiceResult<UserDto>.FailureResult("Email and password are required");
                }

                var user = _unitOfWork.Users.GetByEmail(email.Trim().ToLower());

                if (user == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Invalid email or password");
                }

                // Check if user has a password set (might be Google-only user)
                if (string.IsNullOrEmpty(user.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("This account uses Google sign-in. Please sign in with Google.");
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
                return ServiceResult<UserDto>.SuccessResult(userDto, "Login successful");
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

        public ServiceResult<UserDto> RegisterUser(RegisterDto registerDto, SessionDto sessionInfo)
        {
            try
            {
                // Validate input
                if (registerDto == null)
                {
                    return ServiceResult<UserDto>.FailureResult("Registration data is required");
                }

                if (string.IsNullOrEmpty(registerDto.Name) ||
                    string.IsNullOrEmpty(registerDto.Email) ||
                    string.IsNullOrEmpty(registerDto.Password))
                {
                    return ServiceResult<UserDto>.FailureResult("Name, email, and password are required");
                }

                // Validate password strength
                if (registerDto.Password.Length < 8)
                {
                    return ServiceResult<UserDto>.FailureResult("Password must be at least 8 characters");
                }

                // Check for duplicate email
                if (_unitOfWork.Users.EmailExists(registerDto.Email.Trim().ToLower()))
                {
                    return ServiceResult<UserDto>.FailureResult("An account with this email already exists");
                }

                // Create new user
                var user = new User
                {
                    Name = registerDto.Name.Trim(),
                    Email = registerDto.Email.Trim().ToLower(),
                    Password = EncryptionHelper.HashPassword(registerDto.Password),
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
