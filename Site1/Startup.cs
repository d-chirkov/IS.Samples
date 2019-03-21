using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using IdentityServer3.Core;

// Конфигурация происходит в классе Startup, фактически добавляется middleware, так что добавляем ссылку на owin
[assembly: OwinStartup(typeof(Site1.Startup))]

namespace Site1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Это нужно для защиты от CSRF-атак
            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
            // А это нужно, чтобы ключи Claims-ов пользователя имели адекыватные названия
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            app
                // Используем cookie аутентификацию
                .UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                })
                .UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    // адрес сервера аутентификации
                    Authority = "https://localhost:44301/identity",

                    // идентификатор данного клиента, можно найти в IS.Clients
                    ClientId = "site1",

                    ClientSecret = "secret",

                    // адрес, на который перенаправляем после аутентификации, совпадает с соответствующим в IS.Clients
                    RedirectUri = "http://localhost:51542/",

                    // токен, который запрашиваем, связано со значением Flows.Implicit в IS.Clients
                    ResponseType = "code id_token",

                    // адрес, на который редиректит после выхода, совпадает с соответствующим в IS.Clients
                    PostLogoutRedirectUri = "http://localhost:51542/",
                    SignInAsAuthenticationType = "Cookies",

                    // Фактически обработчики различных событий
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // обработчик события, когда после аутентификации сформировались Claims-ы пользователя
                        SecurityTokenValidated = n =>
                        {
                            // Для того, чтобы выход работал нормально, надо в Claim-ы пользователя добавить токен идентификации
                            var id = n.AuthenticationTicket.Identity;
                            // Копируем созданные Claims
                            var nid = new ClaimsIdentity(id.AuthenticationType);
                            nid.AddClaims(id.Claims);
                            // И добавляем токен идентификации
                            nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            // Заменяем старыем Claim-ы новыми
                            n.AuthenticationTicket = new AuthenticationTicket(
                                nid,
                                n.AuthenticationTicket.Properties);
                            return Task.FromResult(0);
                        },

                        // Нам надо обработать событие выхода пользователя
                        RedirectToIdentityProvider = n =>
                        {
                            // Это взято чисто из примера: https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
                            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                            {
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                                if (idTokenHint != null)
                                {
                                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                }
                            }

                            return Task.FromResult(0);
                        }
                    }
                });

        }
    }
}