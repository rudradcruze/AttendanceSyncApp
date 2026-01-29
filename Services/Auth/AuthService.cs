using System;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Services.Auth
{
    /// <summary>
    /// Service for handling user authentication and registration.
    /// Supports Google OAuth, traditional email/password, and admin authentication.
    /// Manages user sessions and account linking.
    /// </summary>
    public class AuthService : IAuthService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;
        /// Google authentication service for OAuth validation.
        private readonly IGoogleAuthService _googleAuthService;
        /// Session management service for user sessions.
        private readonly ISessionService _sessionService;

        /// <summary>
        /// Initializes a new AuthService with required dependencies.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        /// <param name="googleAuthService">The Google authentication service.</param>
        /// <param name="sessionService">The session management service.</param>
        public AuthService(
            IAuthUnitOfWork unitOfWork,
            IGoogleAuthService googleAuthService,
            ISessionService sessionService)
        {
            _unitOfWork = unitOfWork;
            _googleAuthService = googleAuthService;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Authenticates a user via Google OAuth.
        /// Auto-creates user account on first sign-in or links Google account to existing email user.
        /// </summary>
        /// <param name="googleAuth">The Google authentication token data.</param>
        /// <param name="sessionInfo">Session information for creating user session.</param>
        /// <returns>User details with session token, or failure result.</returns>
        public ServiceResult<UserDto> LoginWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                // Validate Google ID token with Google
                var googleResult = _googleAuthService.ValidateIdToken(googleAuth.IdToken);
                if (!googleResult.Success)
                {
                    return ServiceResult<UserDto>.FailureResult(googleResult.Message);
                }

                var googleUser = googleResult.Data;

                // Find existing user by Google ID or email
                var user = _unitOfWork.Users.GetByGoogleId(googleUser.GoogleId)
                        ?? _unitOfWork.Users.GetByEmail(googleUser.Email);

                if (user == null)
                {
                    // Create new user account on first Google sign-in
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

        /// <summary>
        /// Authenticates an admin user with email and password.
        /// Only users with ADMIN role can log in through this endpoint.
        /// </summary>
        /// <param name="email">The admin's email address.</param>
        /// <param name="password">The admin's password.</param>
        /// <param name="sessionInfo">Session information for creating user session.</param>
        /// <returns>Admin user details with session token, or failure result.</returns>
        public ServiceResult<UserDto> LoginAdmin(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                // Validate credentials are provided
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

        /// <summary>
        /// Authenticates a regular user with email and password.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="sessionInfo">Session information for creating user session.</param>
        /// <returns>User details with session token, or failure result.</returns>
        public ServiceResult<UserDto> LoginUser(string email, string password, SessionDto sessionInfo)
        {
            try
            {
                // Validate credentials are provided
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

        /// <summary>
        /// Registers a new user via Google OAuth.
        /// Fails if an account with the Google email already exists.
        /// </summary>
        /// <param name="googleAuth">The Google authentication token data.</param>
        /// <param name="sessionInfo">Session information for creating user session.</param>
        /// <returns>New user details with session token, or failure result.</returns>
        public ServiceResult<UserDto> RegisterWithGoogle(GoogleAuthDto googleAuth, SessionDto sessionInfo)
        {
            try
            {
                // Validate Google ID token
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

        /// <summary>
        /// Registers a new user with email and password.
        /// Validates password strength and checks for duplicate emails.
        /// </summary>
        /// <param name="registerDto">The registration data including name, email, and password.</param>
        /// <param name="sessionInfo">Session information for creating user session.</param>
        /// <returns>New user details with session token, or failure result.</returns>
        public ServiceResult<UserDto> RegisterUser(RegisterDto registerDto, SessionDto sessionInfo)
        {
            try
            {
                // Validate registration data
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

        /// <summary>
        /// Logs out a user by ending their session.
        /// </summary>
        /// <param name="sessionToken">The session token to invalidate.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult Logout(string sessionToken)
        {
            // Delegate to session service to end session
            return _sessionService.EndSession(sessionToken);
        }

        /// <summary>
        /// Retrieves the currently authenticated user by their session token.
        /// </summary>
        /// <param name="sessionToken">The session token.</param>
        /// <returns>Current user details, or failure if session is invalid.</returns>
        public ServiceResult<UserDto> GetCurrentUser(string sessionToken)
        {
            try
            {
                // Validate session is active
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

        /// <summary>
        /// Validates if a session token is active.
        /// </summary>
        /// <param name="sessionToken">The session token to validate.</param>
        /// <returns>True if session is valid and active, false otherwise.</returns>
        public bool ValidateSession(string sessionToken)
        {
            // Check if session exists and is active
            var result = _sessionService.GetActiveSession(sessionToken);
            return result.Success;
        }

        /// <summary>
        /// Maps a User entity to a UserDto for client responses.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="sessionToken">The session token to include in DTO.</param>
        /// <returns>User DTO with session token.</returns>
        private UserDto MapToUserDto(User user, string sessionToken)
        {
            // Map user properties to DTO
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
