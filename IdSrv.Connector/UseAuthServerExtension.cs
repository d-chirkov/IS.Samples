namespace IdSrv.Connector
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Helpers;
    using IdentityModel.Client;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;
    using Owin;

    public static class UseAuthServerExtension
    {
        private static Dictionary<IAppBuilder, AuthServerConfiguration> Configurations { get; set; } =
            new Dictionary<IAppBuilder, AuthServerConfiguration>();

        private static AuthServerConfiguration GetConfigurationFor(IAppBuilder app)
        {
            if (!Configurations.ContainsKey(app))
            {
                Configurations[app] = new AuthServerConfiguration();
            }
            return Configurations[app];
        }

        public static IAppBuilder UseAuthServer(this IAppBuilder app, string address)
        {
            AuthServerConfiguration config = GetConfigurationFor(app);
            config.IdSrvAddress = address;
            return TryConfigureAuthServer(app, config);
        }

        public static IAppBuilder WithClientId(this IAppBuilder app, string clientId)
        {
            AuthServerConfiguration config = GetConfigurationFor(app);
            config.ClientId = clientId;
            return TryConfigureAuthServer(app, config);
        }

        public static IAppBuilder WithClientSecret(this IAppBuilder app, string clientSecret)
        {
            AuthServerConfiguration config = GetConfigurationFor(app);
            config.ClientSecret = clientSecret;
            return TryConfigureAuthServer(app, config);
        }

        public static IAppBuilder WithOwnAddress(this IAppBuilder app, string ownAddress)
        {
            AuthServerConfiguration config = GetConfigurationFor(app);
            config.OwnAddress = ownAddress;
            return TryConfigureAuthServer(app, config);
        }

        public static IAppBuilder WithWebForms(this IAppBuilder app)
        {
            AuthServerConfiguration config = GetConfigurationFor(app);
            config.UseWebForms = true;
            return TryConfigureAuthServer(app, config);
        }

        private static IAppBuilder TryConfigureAuthServer(IAppBuilder app, AuthServerConfiguration config)
        {
            if (!config.IsComplete)
            {
                return app;
            }

            // Это нужно для защиты от CSRF-атак
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            // А это нужно, чтобы ключи Claims-ов пользователя имели адекыватные названия
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

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
                    Authority = config.IdSrvAddress,

                    // идентификатор данного клиента, можно найти в IS.Clients
                    ClientId = config.ClientId,

                    // Секрет приложения-клиента
                    ClientSecret = config.ClientSecret,

                    // адрес, на который перенаправляем после аутентификации, совпадает с соответствующим в IS.Clients
                    RedirectUri = config.OwnAddress,

                    // токен, который запрашиваем, связано со значением Flows.Hybrid в IS.Clients
                    ResponseType = "code id_token token",

                    // адрес, на который редиректит после выхода, совпадает с соответствующим в IS.Clients
                    PostLogoutRedirectUri = config.OwnAddress,
                    SignInAsAuthenticationType = "Cookies",
                    UseTokenLifetime = false,

                    // Фактически обработчики различных событий
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = async n =>
                        {
                            // use the access token to retrieve claims from userinfo
                            var uriToUserInfo = new Uri(config.IdSrvAddress + "/connect/userinfo");
                            var userInfoClient = new UserInfoClient(uriToUserInfo, n.ProtocolMessage.AccessToken);
                            var userInfoResponse = await userInfoClient.GetAsync();

                            string userName = userInfoResponse?.Claims?.FirstOrDefault(c => c.Item1 == "name")?.Item2;

                            if (userName == null)
                            {
                                n.OwinContext.Authentication.SignOut();
                                return;
                            }

                            // Создаём claims-ы пользователя, которые в дальнейшем будут видны в методах контроллера
                            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                            id.AddClaims(n.AuthenticationTicket.Identity.Claims);

                            // имя пользователя (логин)
                            id.AddClaim(new Claim("name", userName));

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

            if (config.UseWebForms)
            {
                app.UseStageMarker(PipelineStage.Authenticate);
            }

            // Api может пригодиться в будущем, на всякий случай оставил, чтобы не затерялось в истории комитов

            //app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            //{
            //    Authority = "https://localhost:44363/identity",
            //    RequiredScopes = new[] { "api1" },
            //    DelayLoadMetadata = true,

            //    ClientId = "api1",
            //    ClientSecret = "secret"
            //});

            return app;
        }
    }
}
