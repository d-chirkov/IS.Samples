namespace IdSrv.AspNet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using IdentityModel.Client;

    public static class IdSrvConnection
    {
        public static string IdSrvAddress { get; set; }

        public static bool UseAutoLogout { get; set; } = false;

        public static async Task<bool> IsAccessBlockedAsync(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            string claimsUserName = GetUserNameFromClaims(await LoadAndGetUserClaimsAsync(httpContext));
            return userName == null ? false : claimsUserName != userName;
        }

        public static bool IsAccessBlocked(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return userName == null ? false : GetUserNameFromClaims(LoadAndGetUserClaims(httpContext)) != userName;
        }

        public static async Task<string> GetUserNameAsync(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return
                userName == null ? null :
                GetUserNameFromClaims(await LoadAndGetUserClaimsAsync(httpContext)) == userName ? userName :
                null;
        }

        public static string GetUserName(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return
                userName == null ? null :
                GetUserNameFromClaims(LoadAndGetUserClaims(httpContext)) == userName ? userName :
                null;
        }

        public static async Task<string> GetUserIdAsync(HttpContextBase httpContext)
        {
            return await IsAccessBlockedAsync(httpContext) ? null : GetUserIdFromContext(httpContext);
        }

        public static string GetUserId(HttpContextBase httpContext)
        {
            return IsAccessBlocked(httpContext) ? null : GetUserIdFromContext(httpContext);
        }

        public static async Task<IEnumerable<Tuple<string, string>>> GetUserClaimsAsync(HttpContextBase httpContext)
        {
            return await LoadAndGetUserClaimsAsync(httpContext);
        }

        public static IEnumerable<Tuple<string, string>> GetUserClaims(HttpContextBase httpContext)
        {
            return LoadAndGetUserClaims(httpContext);
        }

        private static async Task<IEnumerable<Tuple<string, string>>> LoadAndGetUserClaimsAsync(HttpContextBase httpContext)
        {
            if (!httpContext.Items.Contains("idsrv_user_claims"))
            {
                var userInfoClient = GetUserInfoClient(httpContext);
                var userInfoResponse = await userInfoClient.GetAsync();
                AddUserClaimsToContext(userInfoResponse, httpContext);
            }
            return httpContext.Items["idsrv_user_claims"] as IEnumerable<Tuple<string, string>>;
        }

        private static IEnumerable<Tuple<string, string>> LoadAndGetUserClaims(HttpContextBase httpContext)
        {
            if (!httpContext.Items.Contains("idsrv_user_claims"))
            {
                var userInfoClient = GetUserInfoClient(httpContext);
                if (userInfoClient != null)
                {
                    var userInfoResponse = userInfoClient.GetAsync().Result;
                    AddUserClaimsToContext(userInfoResponse, httpContext);
                }
            }
            return httpContext.Items["idsrv_user_claims"] as IEnumerable<Tuple<string, string>>;
        }

        private static UserInfoClient GetUserInfoClient(HttpContextBase httpContext)
        {
            var claims = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal);
            string accessToken = (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                       ?.FindFirst("access_token")
                       ?.Value;
            if (accessToken == null)
            {
                return null;
            }
            var userInfoClient = new UserInfoClient(new Uri(IdSrvAddress + "/connect/userinfo"), accessToken);
            return userInfoClient;
        }

        private static void AddUserClaimsToContext(UserInfoResponse response, HttpContextBase httpContext)
        {
            if (response != null && !response.IsError && response.Claims != null && response.Claims.Count() > 0)
            {
                httpContext.Items.Add("idsrv_user_claims", response.Claims);
            }
            else if (UseAutoLogout)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
        }

        private static string GetUserIdFromContext(HttpContextBase httpContext)
        {
            return GetValueFromContext(httpContext, "sub");
        }

        private static string GetUserNameFromContext(HttpContextBase httpContext)
        {
            return GetValueFromContext(httpContext, "name");
        }

        private static string GetValueFromContext(HttpContextBase httpContext, string valueName)
        {
            return (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst(valueName)
                    ?.Value;
        }

        private static string GetUserIdFromClaims(IEnumerable<Tuple<string, string>> claims)
        {
            return GetValueFromClaims(claims, "sub");
        }

        private static string GetUserNameFromClaims(IEnumerable<Tuple<string, string>> claims)
        {
            return GetValueFromClaims(claims, "name");
        }

        private static string GetValueFromClaims(IEnumerable<Tuple<string, string>> claims, string valueName)
        {
            return claims?.Where(c => c.Item1 == valueName).FirstOrDefault()?.Item2;
        }
    }
}
