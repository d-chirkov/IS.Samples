// ВАЖНО: в зависимостях стоит версия IdentityModel 2.6.0 - далеко не самая свежая, но для .Net Framework 4.5.1 новее нет
// (IdentityModel нужен для добавления секрета приложения-клиента)

using IdentityModel.Client;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.IdentityModel.Tokens;

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
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            // Собственный адрес
            string ownUri = "http://localhost:57161/";

            // адрес сервера аутентификации
            // Для пользователей из базы данных (пока мнимой)
            string idsrvUri = "https://localhost:44363/identity";

            // Для windows пользователей
            //string idsrvUri = "https://localhost:44384/identity";
            string clientId = "78f36f55-784d-4895-8913-f9a76b807a5c";
            string clientSecret = "123";

            app
                // Используем cookie аутентификацию
                .UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                    ExpireTimeSpan = TimeSpan.FromMinutes(14)
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
                            // use the access token to retrieve claims from userinfo
                            var userInfoClient = new UserInfoClient(new Uri(idsrvUri + "/connect/userinfo"), n.ProtocolMessage.AccessToken);
                            var userInfoResponse = await userInfoClient.GetAsync();

                            // Создаём claims-ы пользователя, которые в дальнейшем будут видны в методах контроллера
                            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                            id.AddClaims(n.AuthenticationTicket.Identity.Claims);

                            // имя пользователя (логин)
                            id.AddClaim(new Claim("name", userInfoResponse.Claims.FirstOrDefault(c => c.Item1 == "name").Item2));

                            // и id_token (нужен для logout-а)
                            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            id.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));
                            n.AuthenticationTicket = new AuthenticationTicket(id, n.AuthenticationTicket.Properties);
                        },

                        // Нам надо обработать событие выхода пользователя
                        RedirectToIdentityProvider = n =>
                        {
                            // Это взято из примера: https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
                            if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnectRequestType.LogoutRequest)
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

            //app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            //{
            //    Authority = "https://localhost:44363/identity",
            //    RequiredScopes = new[] { "api1" },
            //    DelayLoadMetadata = true,

            //    ClientId = "api1",
            //    ClientSecret = "secret"
            //});
        }
    }
}