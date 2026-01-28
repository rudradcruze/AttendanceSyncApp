using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Manages user login sessions for administrators,
    /// including viewing, filtering, and monitoring active sessions.
    /// </summary>
    [AdminAuthorize]
    public class AdminSessionsController : BaseController
    {
        /// <summary>
        /// Unit of work for accessing session data repositories.
        /// </summary>
        private readonly AuthUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes controller with default services.
        /// </summary>
        public AdminSessionsController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
        }

        // GET: AdminSessions/LoginSessions
        public ActionResult LoginSessions()
        {
            // Return the login sessions management view
            return View("~/Views/Admin/LoginSessions.cshtml");
        }

        // GET: AdminSessions/GetAllSessions
        [HttpGet]
        public JsonResult GetAllSessions(int page = 1, int pageSize = 20, string userSearch = null, bool? isActive = null)
        {
            try
            {
                // Get all login sessions as queryable
                var query = _unitOfWork.LoginSessions.GetAll()
                    .AsQueryable();

                // Apply user search filter if provided
                if (!string.IsNullOrEmpty(userSearch))
                {
                    query = query.Where(s => s.User.Name.Contains(userSearch) || s.User.Email.Contains(userSearch));
                }

                // Apply active status filter if provided
                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                // Get total count for pagination
                var totalCount = query.Count();

                // Retrieve paginated sessions ordered by most recent login
                var sessions = query
                    .OrderByDescending(s => s.LoginTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SessionDto
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        UserName = s.User.Name,
                        UserEmail = s.User.Email,
                        Device = s.Device,
                        Browser = s.Browser,
                        IpAddress = s.IPAddress,
                        UserAgent = s.Device + " - " + s.Browser,
                        LoginTime = s.LoginTime,
                        LogoutTime = s.LogoutTime,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt
                    })
                    .ToList();

                // Package result with pagination metadata
                var result = new PagedResultDto<SessionDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = sessions
                };

                return Json(ApiResponse<PagedResultDto<SessionDto>>.Success(result), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return error if session retrieval fails
                return Json(ApiResponse<object>.Fail($"Failed to retrieve sessions: {ex.Message}"), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AdminSessions/GetSession
        [HttpGet]
        public JsonResult GetSession(int id)
        {
            try
            {
                // Retrieve specific session details by ID
                var session = _unitOfWork.LoginSessions.GetAll()
                    .Where(s => s.Id == id)
                    .Select(s => new SessionDto
                    {
                        Id = s.Id,
                        UserId = s.UserId,
                        UserName = s.User.Name,
                        UserEmail = s.User.Email,
                        Device = s.Device,
                        Browser = s.Browser,
                        IpAddress = s.IPAddress,
                        UserAgent = s.Device + " - " + s.Browser,
                        LoginTime = s.LoginTime,
                        LogoutTime = s.LogoutTime,
                        IsActive = s.IsActive,
                        CreatedAt = s.CreatedAt
                    })
                    .FirstOrDefault();

                // Return error if session not found
                if (session == null)
                {
                    return Json(ApiResponse<SessionDto>.Fail("Session not found"), JsonRequestBehavior.AllowGet);
                }

                // Return session details
                return Json(ApiResponse<SessionDto>.Success(session), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return error if retrieval fails
                return Json(ApiResponse<SessionDto>.Fail($"Failed to retrieve session: {ex.Message}"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Disposes unit of work resources when controller is disposed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
