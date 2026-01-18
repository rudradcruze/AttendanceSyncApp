using System;
using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers.Filters
{
    /// <summary>
    /// Authorization filter to ensure user is authenticated with a valid session
    /// </summary>
    public class AuthorizeUserAttribute : ActionFilterAttribute
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

            // Validate session in database
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

                // Check if user is still active
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
            // Check if this is an AJAX request
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
    }
}
