// Поясняющие комментарии см в IS.Site1

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
using IdentityModel.Client;
using System;
using System.Linq;

[assembly: OwinStartup(typeof(Site2.Startup))]

namespace Site2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // (alice:1234, test:123456, test2:test)
            UserAccessControl.AddAccessTo("2", "3", "4");

            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            // адрес сервера аутентификации
            // Для пользователей из базы данных (пока мнимой)
            string idsrvUri = "https://localhost:44301/identity";

            // Для windows пользователей
            //string idsrvUri = "https://localhost:44384/identity";
            string clientId = "site2";
            string clientSecret = "secret2";
            string ownUri = "http://localhost:51566/";

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
                    ResponseType = "id_token code token",
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
                            id.AddClaims(n.AuthenticationTicket.Identity.Claims);
                            id.AddClaim(userInfoResponse.Claims.First(c => c.Type == Constants.ClaimTypes.Name));
                            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            n.AuthenticationTicket = new AuthenticationTicket(id, n.AuthenticationTicket.Properties);
                        },

                        RedirectToIdentityProvider = n =>
                        {
                            if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Logout)
                            {
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");
                                if (idTokenHint != null)
                                {
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
                });
        }
    }
}