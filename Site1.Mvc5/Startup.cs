// ВАЖНО: в зависимостях стоит версия IdentityModel 2.6.0 - далеко не самая свежая, но для .Net Framework 4.5.1 новее нет
// (IdentityModel нужен для добавления секрета приложения-клиента)

using IdentityModel.Client;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;

// Конфигурация происходит в классе Startup, фактически добавляется middleware, так что добавляем ссылку на owin
[assembly: OwinStartup(typeof(Site1.Mvc5.Startup))]

namespace Site1.Mvc5
{
    public static class OidcClaimTypes
    {
        public const string Subject = "sub";
        public const string Name = "name";
    }

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Это нужно для защиты от CSRF-атак
            AntiForgeryConfig.UniqueClaimTypeIdentifier = OidcClaimTypes.Subject;
            // А это нужно, чтобы ключи Claims-ов пользователя имели адекыватные названия
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            // Собственный адрес
            string ownUri = "http://localhost:57161/";

            // адрес сервера аутентификации
            // Для пользователей из базы данных (пока мнимой)
            //string idsrvUri = "https://localhost:44301/identity";

            // Для windows пользователей
            string idsrvUri = "https://localhost:44384/identity";
            string clientId = "site1.mvc5";
            string clientSecret = "123";

            app
                // Используем cookie аутентификацию
                .UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                })
                .UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    // Задаём адрес identity server
                    Authority = idsrvUri,

                    // идентификатор данного клиента, можно найти в IS.Clients
                    ClientId = clientId,

                    // Секрет приложения-клиента
                    ClientSecret = clientSecret,

                    // адрес, на который перенаправляем после аутентификации, совпадает с соответствующим в IS.Clients
                    RedirectUri = ownUri,

                    // токен, который запрашиваем, связано со значением Flows.Hybrid в IS.Clients
                    ResponseType = "code id_token token",

                    // адрес, на который редиректит после выхода, совпадает с соответствующим в IS.Clients
                    PostLogoutRedirectUri = ownUri,
                    SignInAsAuthenticationType = "Cookies",
                    UseTokenLifetime = false,
                    // Фактически обработчики различных событий
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = async n =>
                        {
                            // use the code to get the access and refresh token
                            var tokenClient = new TokenClient(
                                idsrvUri + "/connect/token",
                                clientId,
                                clientSecret);

                            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

                            if (tokenResponse.IsError)
                            {
                                throw new Exception(tokenResponse.Error);
                            }

                            // use the access token to retrieve claims from userinfo
                            var userInfoClient = new UserInfoClient(new Uri(idsrvUri + "/connect/userinfo").ToString());
                            var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);

                            // Создаём claims-ы пользователя, которые в дальнейшем будут видны в методах контроллера
                            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                            id.AddClaims(n.AuthenticationTicket.Identity.Claims);

                            // имя пользователя (логин)
                            id.AddClaim(userInfoResponse.Claims.First(c => c.Type == OidcClaimTypes.Name));

                            // и id_token (нужен для logout-а)
                            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            n.AuthenticationTicket = new AuthenticationTicket(id, n.AuthenticationTicket.Properties);
                        },

                        // Нам надо обработать событие выхода пользователя
                        RedirectToIdentityProvider = n =>
                        {
                            // Это взято из примера: https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
                            if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Logout)
                            {
                                // Вот тут нам нужен id_token, который мы добавляли в claims-ы пользователя чуть выше
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                                if (idTokenHint != null)
                                {
                                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                }
                            }

                            return Task.FromResult(0);
                        },

                        // Исправляет баг, см. https://github.com/IdentityServer/IdentityServer3/issues/542
                        AuthenticationFailed = n =>
                        {
                            if (n.Exception.Message.Contains("IDX21323"))
                            {
                                n.SkipToNextMiddleware();
                            }

                            return Task.FromResult(0);
                        }
                    }
                });

        }
    }
}