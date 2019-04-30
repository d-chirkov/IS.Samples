using Site1.Mvc5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Site1.Mvc5.Attributes
{
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
                // The user is not authenticated
                return false;
            }
            if (this.checkLocalAccess)
            {
                int idsrvUserId = int.Parse((httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                ?.FindFirst(OidcClaimTypes.Subject)
                ?.Value);
                UserProfile userProfile = null;
                using (var context = new AccountsContext())
                {
                    userProfile = context.UserProfiles.Where(p => p.IdSrvId == idsrvUserId).FirstOrDefault();
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