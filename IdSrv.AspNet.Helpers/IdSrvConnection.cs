namespace IdSrv.AspNet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Flurl;
    using IdentityModel.Client;

    public static class IdSrvConnection
    {
        public static string IdSrvAddress { get; set; }

        public static bool UseAutoLogout { get; set; } = false;

        public static async Task<bool> IsAccessBlocked(HttpContextBase httpContext)
        {
            string userLogin = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst("name")
                    ?.Value;
            var userClaims = await LoadAndGetUserClaims(httpContext);
            if (userClaims != null && userClaims.Where(c => c.Item1 == "name").FirstOrDefault()?.Item2 == userLogin)
            {
                return false;
            }
            if (UseAutoLogout)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
            return true;
        }

        public static async Task<string> GetUserName(HttpContextBase httpContext)
        {
            string userLogin = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst("name")
                    ?.Value;
            var userClaims = await LoadAndGetUserClaims(httpContext);
            if (userClaims != null && userClaims.Where(c => c.Item1 == "name").FirstOrDefault()?.Item2 == userLogin)
            {
                return userLogin;
            }
            if (UseAutoLogout)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
            return null;
        }

        public static async Task<string> GetUserId(HttpContextBase httpContext)
        {
            string userId = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst("sub")
                    ?.Value;
            var userClaims = await LoadAndGetUserClaims(httpContext);
            if (userClaims != null && userClaims.Where(c => c.Item1 == "sub").FirstOrDefault()?.Item2 == userId)
            {
                return userId;
            }
            if (UseAutoLogout)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
            return null;
        }

        public static async Task<IEnumerable<Tuple<string, string>>> GetUserClaims(HttpContextBase httpContext)
        {
            var userClaims = await LoadAndGetUserClaims(httpContext);
            if (userClaims == null && UseAutoLogout)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
            return userClaims;
        }

        private static async Task<IEnumerable<Tuple<string, string>>> LoadAndGetUserClaims(HttpContextBase httpContext)
        {
            if (!httpContext.Items.Contains("idsrv_user_claims"))
            {
                string accessToken = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                        ?.FindFirst("access_token")
                        ?.Value;
                var userInfoClient = new UserInfoClient(new Uri(Url.Combine(IdSrvAddress, "/connect/userinfo")), accessToken);
                var userInfoResponse = await userInfoClient.GetAsync();
                if (userInfoResponse != null &&
                    !userInfoResponse.IsError &&
                    userInfoResponse.Claims != null)
                {
                    httpContext.Items.Add("idsrv_user_claims", userInfoResponse.Claims);
                }
                else
                {
                    return null;
                }
            }
            return httpContext.Items["idsrv_user_claims"] as IEnumerable<Tuple<string, string>>;
        }
    }
}
