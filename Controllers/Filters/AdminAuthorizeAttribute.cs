using System;
using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers.Filters
{
    /// <summary>
    /// Authorization filter to ensure user is authenticated and has admin role
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        // Session timeout in hours (default 24 hours)
        private const int SessionTimeoutHours = 24;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cookie = filterContext.HttpContext.Request.Cookies["SessionToken"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                ClearSessionCookie(filterContext.HttpContext.Response);
                HandleUnauthorized(filterContext, "Not authenticated");
                return;
            }

            // Validate session and admin role
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var session = unitOfWork.LoginSessions.GetByToken(cookie.Value);

                if (session == null)
                {
                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session not found");
                    return;
                }

                if (!session.IsActive)
                {
                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session expired");
                    return;
                }

                // Check session timeout (based on LoginTime)
                if (session.LoginTime.AddHours(SessionTimeoutHours) < DateTime.Now)
                {
                    // Mark session as inactive
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    session.UpdatedAt = DateTime.Now;
                    unitOfWork.LoginSessions.Update(session);
                    unitOfWork.SaveChanges();

                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session expired");
                    return;
                }

                var user = unitOfWork.Users.GetById(session.UserId);
                if (user == null || !user.IsActive)
                {
                    // Mark session as inactive
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    session.UpdatedAt = DateTime.Now;
                    unitOfWork.LoginSessions.Update(session);
                    unitOfWork.SaveChanges();

                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "User not found or inactive");
                    return;
                }

                if (user.Role != "ADMIN")
                {
                    HandleForbidden(filterContext);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void ClearSessionCookie(HttpResponseBase response)
        {
            var cookie = new HttpCookie("SessionToken")
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Secure = true,
                Path = "/"
            };
            response.Cookies.Add(cookie);
        }

        private void HandleUnauthorized(ActionExecutingContext filterContext, string message)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.Result = new JsonResult
                {
                    Data = new { Success = false, Message = message },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Auth/Login");
            }
        }

        private void HandleForbidden(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = 403;
                filterContext.Result = new JsonResult
                {
                    Data = new { Success = false, Message = "Admin access required" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(403, "Admin access required");
            }
        }
    }
}
