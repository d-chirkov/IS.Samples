using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(Site3.WebForms.Startup))]

namespace Site3.WebForms
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // А это нужно, чтобы ключи Claims-ов пользователя имели адекыватные названия
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();
            app
                // Используем cookie аутентификацию
                .UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                })
                .UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    SignInAsAuthenticationType = "Cookies",

                    // адрес сервера аутентификации
                    Authority = "https://localhost:44301/identity",

                    // идентификатор данного клиента, можно найти в IS.Clients
                    ClientId = "site3",

                    // адрес, на который перенаправляем после аутентификации, совпадает с соответствующим в IS.Clients
                    RedirectUri = "http://localhost:56140/",

                    // токен, который запрашиваем, связано со значением Flows.Implicit в IS.Clients
                    ResponseType = "id_token",

                    // адрес, на который редиректит после выхода, совпадает с соответствующим в IS.Clients
                    PostLogoutRedirectUri = "http://localhost:56140/",

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
                })

                // !!! ВАЖНО, НЕ ЗАБЫВАТЬ ПРО ЭТУ СТРОКУ ДЛЯ WebForms!!!
                // Это единственное, что отличает настройку для WebForms от MVC5
                .UseStageMarker(PipelineStage.Authenticate);
        }
    }
}
