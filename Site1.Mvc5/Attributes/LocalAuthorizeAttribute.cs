using IdentityModel.Client;
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

            if (this.AccessIsBlocked(httpContext))
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
                return false;
            }

            if (this.checkLocalAccess)
            {
                string idsrvUserId = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst(OidcClaimTypes.Subject)
                    ?.Value;

                string userLogin = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst(OidcClaimTypes.Name)
                    ?.Value;

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

        private bool AccessIsBlocked(HttpContextBase httpContext)
        {
            string userLogin = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst(OidcClaimTypes.Name)
                    ?.Value;
            string accessToken = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst("access_token")
                    ?.Value;
            var userInfoClient = new UserInfoClient(new Uri("https://localhost:44363/identity/connect/userinfo"), accessToken);
            var userInfoResponse = userInfoClient.GetAsync().Result;
            if (userInfoResponse != null &&
                !userInfoResponse.IsError && 
                userInfoResponse.Claims.Where(c => c.Item1 == "name").FirstOrDefault()?.Item2 == userLogin)
            {
                return false;
            }
            return true;
        }
    }
}