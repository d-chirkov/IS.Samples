namespace Site1.Mvc5.Attributes
{
    using Site1.Mvc5.Models;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using IdSrv.AspNet.Helpers;

    public class LocalAuthorizeAttribute : AuthorizeAttribute
    {
        private bool checkLocalAccess;

        public LocalAuthorizeAttribute(bool checkLocalAccess = true)
        {
            this.checkLocalAccess = checkLocalAccess;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                return false;
            }

            if (!IdSrvConnection.IsAccessBlocked(httpContext) && this.checkLocalAccess)
            {
                string idsrvUserId = IdSrvConnection.GetUserId(httpContext);
                string userLogin = IdSrvConnection.GetUserName(httpContext);

                UserProfile userProfile = null;
                using (var context = new AccountsContext())
                {
                    userProfile = 
                        context.UserProfiles.Where(p => p.IdSrvId == idsrvUserId).FirstOrDefault() ??
                        context.UserProfiles.Where(p => p.Login == userLogin).FirstOrDefault();
                }
                if (userProfile == null)
                {
                    httpContext.Response.Redirect("~/Account/AccessDenied");
                }
                return userProfile != null;
            }
            return true;
        }
    }
}