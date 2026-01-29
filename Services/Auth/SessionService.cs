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
    /// Service for managing user login sessions.
    /// Handles session creation, validation, termination, and token generation.
    /// </summary>
    public class SessionService : ISessionService
    {
        /// Unit of work for database operations.
        private readonly IAuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new SessionService with the given unit of work.
        /// </summary>
        /// <param name="unitOfWork">The authentication unit of work.</param>
        public SessionService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates a new login session for a user.
        /// Generates a secure session token and records device/browser information.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="sessionInfo">Session metadata including device, browser, and IP address.</param>
        /// <returns>The created session with token.</returns>
        public ServiceResult<LoginSession> CreateSession(int userId, SessionDto sessionInfo)
        {
            try
            {
                // Create new session entity
                var session = new LoginSession
                {
                    UserId = userId,
                    Device = sessionInfo?.Device,
                    Browser = sessionInfo?.Browser,
                    IPAddress = sessionInfo?.IPAddress,
                    SessionToken = GenerateSessionToken(),
                    LoginTime = DateTime.Now,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.LoginSessions.Add(session);
                _unitOfWork.SaveChanges();

                return ServiceResult<LoginSession>.SuccessResult(session, "Session created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginSession>.FailureResult($"Failed to create session: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves an active session by its token.
        /// Validates that the session exists and is still active.
        /// </summary>
        /// <param name="sessionToken">The session token.</param>
        /// <returns>The active session, or failure if not found/inactive.</returns>
        public ServiceResult<LoginSession> GetActiveSession(string sessionToken)
        {
            try
            {
                // Validate token provided
                if (string.IsNullOrEmpty(sessionToken))
                {
                    return ServiceResult<LoginSession>.FailureResult("Session token is required");
                }

                var session = _unitOfWork.LoginSessions.GetByToken(sessionToken);

                if (session == null)
                {
                    return ServiceResult<LoginSession>.FailureResult("Session not found or expired");
                }

                if (!session.IsActive)
                {
                    return ServiceResult<LoginSession>.FailureResult("Session is no longer active");
                }

                return ServiceResult<LoginSession>.SuccessResult(session);
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginSession>.FailureResult($"Error retrieving session: {ex.Message}");
            }
        }

        /// <summary>
        /// Ends a user session (logout).
        /// Marks the session as inactive and records logout time.
        /// </summary>
        /// <param name="sessionToken">The session token to end.</param>
        /// <returns>Success or failure result.</returns>
        public ServiceResult EndSession(string sessionToken)
        {
            try
            {
                // Validate token provided
                if (string.IsNullOrEmpty(sessionToken))
                {
                    return ServiceResult.FailureResult("Session token is required");
                }

                var session = _unitOfWork.LoginSessions.FirstOrDefault(s => s.SessionToken == sessionToken);

                if (session != null)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    session.UpdatedAt = DateTime.Now;
                    _unitOfWork.LoginSessions.Update(session);
                    _unitOfWork.SaveChanges();
                }

                return ServiceResult.SuccessResult("Session ended successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult($"Error ending session: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a cryptographically secure session token.
        /// </summary>
        /// <returns>A 64-character secure token.</returns>
        public string GenerateSessionToken()
        {
            // Generate secure random token
            return EncryptionHelper.GenerateSecureToken(64);
        }
    }
}
