using System.Web;
using System.Web.Mvc;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers.Filters
{
    /// <summary>
    /// Authorization filter to ensure user is authenticated
    /// </summary>
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cookie = filterContext.HttpContext.Request.Cookies["SessionToken"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                HandleUnauthorized(filterContext);
                return;
            }

            // Validate session in database
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var session = unitOfWork.LoginSessions.GetByToken(cookie.Value);

                if (session == null || !session.IsActive)
                {
                    HandleUnauthorized(filterContext);
                    return;
                }

                // Check if user is still active
                var user = unitOfWork.Users.GetById(session.UserId);
                if (user == null || !user.IsActive)
                {
                    HandleUnauthorized(filterContext);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private void HandleUnauthorized(ActionExecutingContext filterContext)
        {
            // Check if this is an AJAX request
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { Success = false, Message = "Session expired. Please login again." },
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
