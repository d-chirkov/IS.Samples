namespace IdSrv.AspNet.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using IdentityModel.Client;

    /// <summary>
    /// Класс, представляющий подключение к identity server для ASP.NET приложений.
    /// Сделан статическим, так как для одного приложения может быть только одно подключение к identity server.
    /// Содержит вспомогательные методы, позволяющие получить информацию о пользователе identity server, такие как
    /// логин, статус блокировки, идентификатор и т.п.
    /// </summary>
    public static class IdSrvConnection
    {
        /// <summary>
        /// Получает или задает URI-адрес identity server.
        /// </summary>
        public static string IdSrvAddress { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, использовать ли автоматический выход
        /// (перенаправление на страницу выхода) пользователя в случае,
        /// когда нет доступа к его данным (например, если пользователь заблокирован).
        /// </summary>
        public static bool UseAutoLogoutWhenNoAccess { get; set; } = false;

        /// <summary>
        /// Является ли пользователь, к которому относится http-контекст, заблокированным.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// true, если пользователь заблокирован, иначе false.
        /// </returns>
        public static async Task<bool> IsAccessBlockedAsync(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            string claimsUserName = GetUserNameFromClaims(await LoadAndGetUserClaimsAsync(httpContext));
            return userName == null ? false : claimsUserName != userName;
        }

        /// <summary>
        /// Является ли пользователь, к которому относится http-контекст, заблокированным.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// true, если пользователь заблокирован, иначе false.
        /// </returns>
        public static bool IsAccessBlocked(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return userName == null ? false : GetUserNameFromClaims(LoadAndGetUserClaims(httpContext)) != userName;
        }

        /// <summary>
        /// Получить имя пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Имя пользователя, если его удалось получить, либо null в противном случае.
        /// </returns>
        public static async Task<string> GetUserNameAsync(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return
                userName == null ? null :
                GetUserNameFromClaims(await LoadAndGetUserClaimsAsync(httpContext)) == userName ? userName :
                null;
        }

        /// <summary>
        /// Получить имя пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Имя пользователя, если его удалось получить, либо null в противном случае.
        /// </returns>
        public static string GetUserName(HttpContextBase httpContext)
        {
            string userName = GetUserNameFromContext(httpContext);
            return
                userName == null ? null :
                GetUserNameFromClaims(LoadAndGetUserClaims(httpContext)) == userName ? userName :
                null;
        }

        /// <summary>
        /// Получить идентификатор пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Идентификатор пользователя в виде строки, если его удалось получить, либо null в противном случае.
        /// </returns>
        public static async Task<string> GetUserIdAsync(HttpContextBase httpContext)
        {
            string userId = GetUserIdFromContext(httpContext);
            return
                userId == null ? null :
                GetUserIdFromClaims(await LoadAndGetUserClaimsAsync(httpContext)) == userId ? userId :
                null;
        }

        /// <summary>
        /// Получить идентификатор пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Идентификатор пользователя в виде строки, если его удалось получить, либо null в противном случае.
        /// </returns>
        public static string GetUserId(HttpContextBase httpContext)
        {
            string userId = GetUserIdFromContext(httpContext);
            return
                userId == null ? null :
                GetUserIdFromClaims(LoadAndGetUserClaims(httpContext)) == userId ? userId :
                null;
        }

        /// <summary>
        /// Получить набор claims пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Набор claims пользователя (возможно пустой, но не должно такого быть), если его удалось получить, 
        /// либо null в противном случае.
        /// </returns>
        public static async Task<IEnumerable<Tuple<string, string>>> GetUserClaimsAsync(HttpContextBase httpContext)
        {
            return await LoadAndGetUserClaimsAsync(httpContext);
        }

        /// <summary>
        /// Получить набор claims пользователя, к которому относится http-контекст.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Набор claims пользователя (возможно пустой, но не должно такого быть), если его удалось получить, 
        /// либо null в противном случае.
        /// </returns>
        public static IEnumerable<Tuple<string, string>> GetUserClaims(HttpContextBase httpContext)
        {
            return LoadAndGetUserClaims(httpContext);
        }

        /// <summary>
        /// Загрузить и закэшировать claims пользователя для данного http-контекста.
        /// Если claims пользователя ранее был получен для данного http-контекста, то берётся из кэша.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Набор claims пользователя (возможно пустой, но не должно такого быть), если его удалось получить,
        /// либо null в противном случае.
        /// </returns>
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

        /// <summary>
        /// Загрузить и закэшировать claims пользователя для данного http-контекста.
        /// Если claims пользователя ранее был получен для данного http-контекста, то берётся из кэша.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Набор claims пользователя (возможно пустой, но не должно такого быть), если его удалось получить,
        /// либо null в противном случае.
        /// </returns>
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

        /// <summary>
        /// Получить клиента для доступа к информации о пользователе из identity server по пути /connect/userinfo.
        /// Используется access_token, сохранённый в Owin-контексте пользователя при его входе на сайт.
        /// </summary>
        /// <param name="httpContext">
        /// HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.
        /// </param>
        /// <returns>
        /// Клиент для доступа к информации о пользователе, либо null, если кго создать не удалось (например, в случае
        /// если в Owin-контексте отсутствует access_token).
        /// </returns>
        private static UserInfoClient GetUserInfoClient(HttpContextBase httpContext)
        {
            var claims = httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal;
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

        /// <summary>
        /// Добавить новую запись в claims пользователя, закэшированные для данного HTTP-контекста.
        /// Добавление происходит в словарь <see cref="HttpContextBase.Items"/> аргумента <paramref name="response"/>
        /// по ключу "idsrv_user_claims".
        /// </summary>
        /// <param name="response">Ответ клиента, созданного в <see cref="GetUserInfoClient(HttpContextBase)"/>.</param>
        /// <param name="httpContext">HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.</param>
        private static void AddUserClaimsToContext(UserInfoResponse response, HttpContextBase httpContext)
        {
            if (response != null && !response.IsError && response.Claims != null && response.Claims.Count() > 0)
            {
                httpContext.Items.Add("idsrv_user_claims", response.Claims);
            }
            else if (UseAutoLogoutWhenNoAccess)
            {
                httpContext.Request.GetOwinContext().Authentication.SignOut();
            }
        }

        /// <summary>
        /// Получить id пользователя из HTTP-контекста (значение по ключу "sub" в claims пользователя).
        /// </summary>
        /// <param name="httpContext">HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.</param>
        /// <returns>
        /// id пользователя в виде строки, либо null, если его получить не удалось (например, если пользователь
        /// не залогинен).
        /// </returns>
        private static string GetUserIdFromContext(HttpContextBase httpContext)
        {
            return GetValueFromContext(httpContext, "sub");
        }

        /// <summary>
        /// Получить логин пользователя из HTTP-контекста (значение по ключу "name" в claims пользователя).
        /// </summary>
        /// <param name="httpContext">HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.</param>
        /// <returns>
        /// Логин пользователя в виде строки, либо null, если его получить не удалось (например, если пользователь
        /// не залогинен).
        /// </returns>
        private static string GetUserNameFromContext(HttpContextBase httpContext)
        {
            return GetValueFromContext(httpContext, "name");
        }

        /// <summary>
        /// Получить claim пользователя по ключу из HTTP-контекста.
        /// </summary>
        /// <param name="httpContext">HTTP-контекст, передаётся из ASP.NET, по нему метод получает информацию о вошедшем пользователе.</param>
        /// <param name="valueName">Ключ, по которому необходимо получить claim (то есть имя claim-а).</param>
        /// <returns>
        /// Значение claim-а пользователя в виде строки, либо null, если его получить не удалось (например, если пользователь
        /// не залогинен).
        /// </returns>
        private static string GetValueFromContext(HttpContextBase httpContext, string valueName)
        {
            return (httpContext.Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                    ?.FindFirst(valueName)
                    ?.Value;
        }

        /// <summary>
        /// Получить id пользователя непосредственно из claims.
        /// </summary>
        /// <param name="claims">Словарь claims пользователя.</param>
        /// <returns>
        /// id пользователя в виде строки, либо null, если такого claim-а нет в словаре.
        /// </returns>
        private static string GetUserIdFromClaims(IEnumerable<Tuple<string, string>> claims)
        {
            return GetValueFromClaims(claims, "sub");
        }

        /// <summary>
        /// Получить логин пользователя непосредственно из claims.
        /// </summary>
        /// <param name="claims">Словарь claims пользователя.</param>
        /// <returns>
        /// Логин пользователя в виде строки, либо null, если такого claim-а нет в словаре.
        /// </returns>
        private static string GetUserNameFromClaims(IEnumerable<Tuple<string, string>> claims)
        {
            return GetValueFromClaims(claims, "name");
        }

        /// <summary>
        /// Получить значение claim-а пользователя по ключу непосредственно из claims.
        /// </summary>
        /// <param name="claims">Словарь claims пользователя.</param>
        /// <param name="valueName">Ключ, по которому необходимо получить claim (то есть имя claim-а).</param>
        /// <returns>
        /// Значение claim-а пользователя в виде строки, либо null, если claim-а по такому ключу нет в словаре.
        /// </returns>
        private static string GetValueFromClaims(IEnumerable<Tuple<string, string>> claims, string valueName)
        {
            return claims?.Where(c => c.Item1 == valueName).FirstOrDefault()?.Item2;
        }
    }
}
