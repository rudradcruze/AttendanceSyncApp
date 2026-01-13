using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers.Filters
{
    /// <summary>
    /// Authorization filter to ensure user is an admin
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cookie = filterContext.HttpContext.Request.Cookies["SessionToken"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                HandleUnauthorized(filterContext, "Not authenticated");
                return;
            }

            // Validate session and admin role
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var session = unitOfWork.LoginSessions.GetByToken(cookie.Value);

                if (session == null || !session.IsActive)
                {
                    HandleUnauthorized(filterContext, "Session expired");
                    return;
                }

                var user = unitOfWork.Users.GetById(session.UserId);
                if (user == null || !user.IsActive)
                {
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

        private void HandleUnauthorized(ActionExecutingContext filterContext, string message)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
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
