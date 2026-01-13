using System;
using AttandanceSyncApp.Helpers;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Auth;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Auth;

namespace AttandanceSyncApp.Services.Auth
{
    public class SessionService : ISessionService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public SessionService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ServiceResult<LoginSession> CreateSession(int userId, SessionDto sessionInfo)
        {
            try
            {
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

        public ServiceResult<LoginSession> GetActiveSession(string sessionToken)
        {
            try
            {
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

        public ServiceResult EndSession(string sessionToken)
        {
            try
            {
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

        public string GenerateSessionToken()
        {
            return EncryptionHelper.GenerateSecureToken(64);
        }
    }
}
