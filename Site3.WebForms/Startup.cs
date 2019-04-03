using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityServer3.Core;
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            // адрес сервера аутентификации
            // Для пользователей из базы данных (пока мнимой)
            string idsrvUri = "https://localhost:44301/identity";

            // Для windows пользователей
            //string idsrvUri = "https://localhost:44384/identity";
            string clientId = "site3";
            string clientSecret = "secret3";
            string ownUri = "http://localhost:56140/";

            app
                .UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies"
                })
                .UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Authority = idsrvUri,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    RedirectUri = ownUri,
                    ResponseType = "code id_token",
                    PostLogoutRedirectUri = ownUri,
                    SignInAsAuthenticationType = "Cookies",
                    UseTokenLifetime = false,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = async n =>
                        {
                            var tokenClient = new TokenClient(
                                idsrvUri + "/connect/token",
                                clientId,
                                clientSecret);

                            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

                            if (tokenResponse.IsError)
                            {
                                throw new Exception(tokenResponse.Error);
                            }

                            var userInfoClient = new UserInfoClient(new Uri(idsrvUri + "/connect/userinfo").ToString());
                            var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);
                            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                            id.AddClaim(n.AuthenticationTicket.Identity.FindFirst(Constants.ClaimTypes.Subject));
                            id.AddClaim(userInfoResponse.Claims.First(c => c.Type == Constants.ClaimTypes.Name));
                            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            n.AuthenticationTicket = new AuthenticationTicket(id, n.AuthenticationTicket.Properties);
                        },

                        RedirectToIdentityProvider = n =>
                        {
                            n.ProtocolMessage.RedirectUri = ownUri;
                            if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Logout)
                            {
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");
                                if (idTokenHint != null)
                                {
                                    n.ProtocolMessage.PostLogoutRedirectUri = ownUri;
                                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                }
                            }

                            return Task.FromResult(0);
                        },

                        AuthenticationFailed = n =>
                        {
                            if (n.Exception.Message.StartsWith("OICE_20004") || n.Exception.Message.Contains("IDX10311"))
                            {
                                n.SkipToNextMiddleware();
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
