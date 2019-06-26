using Microsoft.Owin;

[assembly: OwinStartup(typeof(IdSrv.Server.Startup))]

namespace IdSrv.Server
{
    using Owin;
    using Microsoft.Owin.Security.WsFederation;
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;
    using Services;
    using Serilog;
    using IdSrv.Server.Repositories;
    using System.Collections.Generic;
    using IdentityServer.WindowsAuthentication.Configuration;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdSrv.Server.Repositories.Abstractions;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureIdentityServer(app);
            this.ConfigureWindowsIdentityServer(app);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}\\IS.Log.txt")
                .CreateLogger();
        }

        void ConfigureIdentityServer(IAppBuilder app)
        {
            // Настраиваем сервер аутентификации
            app.Map("/identity", idsrvApp =>
            {
                var tokenApiScope = new Scope
                {
                    Name = "api1",
                    Type = ScopeType.Resource,
                    ScopeSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    }
                };

                var scopes = StandardScopes.All.Concat(new[] { tokenApiScope });

                var factory = new IdentityServerServiceFactory().UseInMemoryScopes(scopes);

                var clientRepository = new RestClientRepository("https://localhost:44397/Api/Client/");
                var clientStore = new CustomClientStore(clientRepository, scopes);
                factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);

                var userRepository = new RestUserRepository("https://localhost:44397/Api/User/");
                var userService = new CustomUserService(userRepository);
                factory.UserService = new Registration<IUserService>(resolver => userService);

                var tokenHandleStore = new CustomTokenHandleStore(userRepository, clientRepository);
                factory.TokenHandleStore = new Registration<ITokenHandleStore>(resolver => tokenHandleStore);

                //var sessionValidator = new CustomAuthenticationSessionValidator(userRepository);
                //factory.AuthenticationSessionValidator = new Registration<IAuthenticationSessionValidator>(resolver => sessionValidator);

                // Устанавливаем наш CustomViewService, чтобы после выхода пользователя в сообщении не выводилось
                // неправильное имя клиентского приложения (выводится то, на котором был произведён вход).
                factory.ViewService = new DefaultViewServiceRegistration<CustomViewService>();

                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    LoggingOptions = new LoggingOptions
                    {
                        EnableWebApiDiagnostics = true,
                        WebApiDiagnosticsIsVerbose = true
                    },
                    // Имя сервера аутентификации, можно будет наблюдать вверху страницы
                    SiteName = "Embedded IdentityServer",

                    IssuerUri = "https://localhost:44363",

                    // Загружаем сертификат
                    SigningCertificate = Startup.LoadCertificate(),

                    // Для примера все пользователи и клиенты берутся из памяти
                    Factory = factory,

                    AuthenticationOptions = new AuthenticationOptions
                    {
                        // После нажатия кнопки "Выход" на сайте, мы автоматически перенапрам пользователя (см. поле Client.PostLogoutRedirectUris)
                        EnablePostSignOutAutoRedirect = true,
                        RequireSignOutPrompt = false,
                        EnableSignOutPrompt = false,

                        // После нажатия на кнопку выхода, выведется страница с ссобщением об успешном выходе.
                        // Здесь указывается, через сколько секунд после вывода этой страницы надо перенаправить пользоватя обратно на сайт.
                        PostSignOutAutoRedirectDelay = 3,

                        // Указываем время жизни куков сервера аутентификации, необходимо, можно ставить 
                        // равным или меньшим значия Client.IdentityTokenLifetime (если будет больше, то сервер аутентификации не будет
                        // забывать логины и пароли пользователей)
                        CookieOptions = new IdentityServer3.Core.Configuration.CookieOptions
                        {
                            // Время жизни сессий клиентов
                            ExpireTimeSpan = TimeSpan.FromMinutes(14),
                        }
                    }
                });
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

        void ConfigureWindowsIdentityServer(IAppBuilder app)
        {
            app.Map("/windows", Startup.ConfigureWindowsTokenProvider);

            IEnumerable<Scope> scopes = StandardScopes.All.Concat(new[]
            {
                new Scope
                {
                    Name = "idmgr",
                    DisplayName = "IdentityManager",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                    ShowInDiscoveryDocument = false,

                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Name),
                        new ScopeClaim(Constants.ClaimTypes.Role)
                    }
                }
            });

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryScopes(scopes);

            var clientRepository = new RestClientRepository("https://localhost:44397/Api/Client/");
            var userRepository = new RestUserRepository("https://localhost:44397/Api/User/");
            var clientStore = new CustomClientStore(clientRepository, scopes, isWindowsAuth: true);
            factory.Register(new Registration<IUserRepository>(r => userRepository));
            factory.Register(new Registration<IClientRepository>(r => clientRepository));
            factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);
            factory.UserService = new Registration<IUserService>(typeof(ExternalRegistrationUserService));
            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator, CustomGrantValidator>());

            app.Map("/winidentity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SigningCertificate = Startup.LoadCertificate(),
                    Factory = factory,
                    AuthenticationOptions = new AuthenticationOptions
                    {
                        EnableLocalLogin = false,
                        IdentityProviders = ConfigureIdentityProviders,

                        // После нажатия кнопки "Выход" на сайте, мы автоматически перенапрам пользователя (см. поле Client.PostLogoutRedirectUris)
                        EnablePostSignOutAutoRedirect = true,

                        // После нажатия на кнопку выхода, выведется страница с ссобщением об успешном выходе.
                        // Здесь указывается, через сколько секунд после вывода этой страницы надо перенаправить пользоватя обратно на сайт.
                        PostSignOutAutoRedirectDelay = 3,

                        // Указываем время жизни куков сервера аутентификации, необходимо, можно ставить 
                        // равным или меньшим значия Client.IdentityTokenLifetime (если будет больше, то сервер аутентификации не будет
                        // забывать логины и пароли пользователей)
                        CookieOptions = new IdentityServer3.Core.Configuration.CookieOptions
                        {
                            // Время жизни сессий клиентов
                            ExpireTimeSpan = TimeSpan.FromMinutes(5)
                        }
                    },

                });
            });

            var options = new IdentityServerOptions
            {
                SigningCertificate = LoadCertificate(),
                Factory = factory,
                AuthenticationOptions = new AuthenticationOptions
                {
                    EnableLocalLogin = false,
                    IdentityProviders = ConfigureIdentityProviders
                },
            };

            app.UseIdentityServer(options);
        }

        private static void ConfigureWindowsTokenProvider(IAppBuilder app)
        {
            app.UseWindowsAuthenticationService(new WindowsAuthenticationOptions
            {
                IdpReplyUrl = "https://localhost:44363/was",
                SigningCertificate = LoadCertificate(),
                EnableOAuth2Endpoint = false
            });
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var wsFederation = new WsFederationAuthenticationOptions
            {
                AuthenticationType = "windows",
                Caption = "Windows",
                SignInAsAuthenticationType = signInAsType,

                MetadataAddress = "https://localhost:44363/windows",
                Wtrealm = "urn:idsrv3",
                Notifications = new WsFederationAuthenticationNotifications
                {
                    // ignore signout requests (we can't sign out of Windows)
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.IsSignOutMessage)
                        {
                            n.HandleResponse();
                        }
                        return Task.FromResult(0);
                    }
                }
            };
            app.UseWsFederationAuthentication(wsFederation);
        }

        static X509Certificate2 LoadCertificate()
        {
            // Тестовый сертификат, взят с сайта identityserver3, можно ставить свой. 
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
