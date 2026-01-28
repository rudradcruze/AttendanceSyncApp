using System;
using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers.Filters
{
    /// <summary>
    /// Authorization filter to ensure user is authenticated and has admin role.
    /// Validates session token, checks session timeout, and verifies admin privileges.
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        /// Session timeout in hours (default 24 hours).
        private const int SessionTimeoutHours = 24;

        /// <summary>
        /// Executes before each action to validate admin authentication and authorization.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Retrieve session token from cookie
            var cookie = filterContext.HttpContext.Request.Cookies["SessionToken"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                // No token found, redirect to login
                ClearSessionCookie(filterContext.HttpContext.Response);
                HandleUnauthorized(filterContext, "Not authenticated");
                return;
            }

            // Validate session and admin role
            using (var unitOfWork = new AuthUnitOfWork())
            {
                // Look up session by token
                var session = unitOfWork.LoginSessions.GetByToken(cookie.Value);

                // Session token not found in database
                if (session == null)
                {
                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session not found");
                    return;
                }

                // Session has been marked as inactive
                if (!session.IsActive)
                {
                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session expired");
                    return;
                }

                // Check if session has exceeded timeout period
                if (session.LoginTime.AddHours(SessionTimeoutHours) < DateTime.Now)
                {
                    // Mark session as inactive and record logout time
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    session.UpdatedAt = DateTime.Now;
                    unitOfWork.LoginSessions.Update(session);
                    unitOfWork.SaveChanges();

                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "Session expired");
                    return;
                }

                // Retrieve user associated with session
                var user = unitOfWork.Users.GetById(session.UserId);
                if (user == null || !user.IsActive)
                {
                    // User deleted or deactivated, invalidate session
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    session.UpdatedAt = DateTime.Now;
                    unitOfWork.LoginSessions.Update(session);
                    unitOfWork.SaveChanges();

                    ClearSessionCookie(filterContext.HttpContext.Response);
                    HandleUnauthorized(filterContext, "User not found or inactive");
                    return;
                }

                // Verify user has admin role
                if (user.Role != "ADMIN")
                {
                    HandleForbidden(filterContext);
                    return;
                }
            }

            // All checks passed, continue to action
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Clears the session token cookie by setting expiration to the past.
        /// </summary>
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

        /// <summary>
        /// Handles unauthorized access by returning JSON for AJAX or redirecting to login.
        /// </summary>
        private void HandleUnauthorized(ActionExecutingContext filterContext, string message)
        {
            // Return JSON response for AJAX requests
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
                // Redirect to login page for normal requests
                filterContext.Result = new RedirectResult("~/Auth/Login");
            }
        }

        /// <summary>
        /// Handles forbidden access when user lacks admin privileges.
        /// </summary>
        private void HandleForbidden(ActionExecutingContext filterContext)
        {
            // Return JSON response for AJAX requests
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
                // Show access denied page for normal requests
                var result = new ViewResult
                {
                    ViewName = "AccessDenied"
                };
                result.ViewBag.Message = "Admin access required to view this page.";
                filterContext.Result = result;
            }
        }
    }
}
