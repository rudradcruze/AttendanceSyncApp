using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminSessionsController : BaseController
    {
        private readonly AuthUnitOfWork _unitOfWork;

        public AdminSessionsController() : base()
        {
            _unitOfWork = new AuthUnitOfWork();
        }

        // GET: AdminSessions/LoginSessions
        public ActionResult LoginSessions()
        {
            return View("~/Views/Admin/LoginSessions.cshtml");
        }

        // GET: AdminSessions/GetAllSessions
        [HttpGet]
        public JsonResult GetAllSessions(int page = 1, int pageSize = 20, string userSearch = null, bool? isActive = null)
        {
            try
            {
                var query = _unitOfWork.LoginSessions.GetAll()
                    .AsQueryable();

                // Apply user search filter
                if (!string.IsNullOrEmpty(userSearch))
                {
                    query = query.Where(s => s.User.Name.Contains(userSearch) || s.User.Email.Contains(userSearch));
                }

                // Apply active filter
                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                var totalCount = query.Count();

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
                return Json(ApiResponse<object>.Fail($"Failed to retrieve sessions: {ex.Message}"), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: AdminSessions/GetSession
        [HttpGet]
        public JsonResult GetSession(int id)
        {
            try
            {
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

                if (session == null)
                {
                    return Json(ApiResponse<SessionDto>.Fail("Session not found"), JsonRequestBehavior.AllowGet);
                }

                return Json(ApiResponse<SessionDto>.Success(session), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse<SessionDto>.Fail($"Failed to retrieve session: {ex.Message}"), JsonRequestBehavior.AllowGet);
            }
        }

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
