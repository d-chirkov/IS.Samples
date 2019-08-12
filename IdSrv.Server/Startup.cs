using Microsoft.Owin;

[assembly: OwinStartup(typeof(IdSrv.Server.Startup))]

namespace IdSrv.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using IdentityServer.WindowsAuthentication.Configuration;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.WebApi.RestClient;
    using IdSrv.Server.Loggers;
    using IdSrv.Server.Loggers.Abstractions;
    using IdSrv.Server.Repositories;
    using IdSrv.Server.Repositories.Abstractions;
    using IdSrv.Server.Services;
    using IdSrv.Server.Services.LoggedDecorators;
    using Microsoft.Owin.Security.WsFederation;
    using Owin;
    using Serilog;

    /// <summary>
    /// Класс для настройки Owin.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Получает или задает URL-адрес WebApi-сервиса для доступа к клиентам и пользователям identity server.
        /// </summary>
        public string WebApiURL { get; set; }

        /// <summary>
        /// Сконфигурировать приложение Owin.
        /// </summary>
        /// <param name="app">Сборщик приложения.</param>
        public void Configuration(IAppBuilder app)
        {
            this.WebApiURL = "https://localhost:44397";
            this.ConfigureIdentityServer(app);
            this.ConfigureWindowsIdentityServer(app);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}\\IdSrv.Server.System.log")
                .CreateLogger();
        }

        /// <summary>
        /// Сконфигурировать приложение Owin для работы с windows-аутентификацией через URI /was.
        /// </summary>
        /// <param name="app">Owin-приложение.</param>
        private static void ConfigureWindowsTokenProvider(IAppBuilder app)
        {
            app.UseWindowsAuthenticationService(new WindowsAuthenticationOptions
            {
                IdpReplyUrl = "https://localhost:44363/was",
                SigningCertificate = LoadCertificate(),
                EnableOAuth2Endpoint = false,
            });
        }

        /// <summary>
        /// Получить сертификат для identity server, путь к сертификату жёстко прописан внутри кода
        /// метода (надо поменять, тестовая реализация).
        /// </summary>
        /// <returns>Считанный сертификат.</returns>
        private static X509Certificate2 LoadCertificate()
        {
            // Тестовый сертификат, взят с сайта identityserver3, можно ставить свой.
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }

        /// <summary>
        /// Настроить Owin-приложение как identity server для работы с обычными (не windows) пользователями.
        /// </summary>
        /// <param name="app">Owin-приложение.</param>
        private void ConfigureIdentityServer(IAppBuilder app)
        {
            // Настраиваем сервер аутентификации
            app.Map(
                "/identity",
                idsrvApp =>
                {
                    var tokenApiScope = new Scope
                    {
                        Name = "api1",
                        Type = ScopeType.Resource,
                        ScopeSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256()),
                        },
                    };

                    var scopes = StandardScopes.All.Concat(new[] { tokenApiScope });

                    var factory = new IdentityServerServiceFactory().UseInMemoryScopes(scopes);

                    var clientRepository = new RestClientRepository(new ClientRestClient(this.WebApiURL));
                    var clientStore = new CustomClientStore(clientRepository, scopes);
                    factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);

                    var userRepository = new RestUserRepository(new UserRestClient(this.WebApiURL));
                    var logger = new FileAuthLogger($"{AppDomain.CurrentDomain.BaseDirectory}\\IdSrv.Server.identity.log");
                    var userService = new LoggedUserServiceDecorator(new CustomUserService(userRepository), logger);
                    factory.UserService = new Registration<IUserService>(resolver => userService);

                    // Устанавливаем наш CustomViewService, чтобы после выхода пользователя в сообщении не выводилось
                    // неправильное имя клиентского приложения (выводится то, на котором был произведён вход).
                    factory.ViewService = new DefaultViewServiceRegistration<CustomViewService>();

                    idsrvApp.UseIdentityServer(new IdentityServerOptions
                    {
                        LoggingOptions = new LoggingOptions
                        {
                            EnableWebApiDiagnostics = true,
                            WebApiDiagnosticsIsVerbose = true,
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
                                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                            },
                        },
                    });
                });

            /*app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44363/identity",
                RequiredScopes = new[] { "api1" },
                DelayLoadMetadata = true,

                ClientId = "api1",
                ClientSecret = "secret"
            });*/
        }

        /// <summary>
        /// Настроить Owin-приложение как identity server для работы с windows пользователями.
        /// </summary>
        /// <param name="app">Owin-приложение.</param>
        private void ConfigureWindowsIdentityServer(IAppBuilder app)
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
                        new ScopeClaim(Constants.ClaimTypes.Role),
                    },
                },
            });

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryScopes(scopes);

            var clientRepository = new RestClientRepository(new ClientRestClient(this.WebApiURL));
            var userRepository = new RestUserRepository(new UserRestClient(this.WebApiURL));
            var clientStore = new CustomClientStore(clientRepository, scopes, isWindowsAuth: true);
            var logger = new FileAuthLogger($"{AppDomain.CurrentDomain.BaseDirectory}\\IdSrv.Server.winidentity.log");
            factory.Register(new Registration<IAuthLogger>(r => logger));
            factory.Register(new Registration<IUserRepository>(r => userRepository));
            factory.Register(new Registration<IClientRepository>(r => clientRepository));
            factory.ClientStore = new Registration<IClientStore>(r => clientStore);
            factory.Register(new Registration<ExternalRegistrationUserService>());
            factory.UserService = new Registration<IUserService>(r =>
                new LoggedUserServiceDecorator(r.Resolve<ExternalRegistrationUserService>(), r.Resolve<IAuthLogger>()));
            factory.Register(new Registration<CustomGrantValidator>());
            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator>(r =>
                new LoggedGrantValidatorDecorator(r.Resolve<CustomGrantValidator>(), r.Resolve<IAuthLogger>())));
            factory.ViewService = new DefaultViewServiceRegistration<CustomViewService>();
            app.Map(
                "/winidentity",
                idsrvApp =>
                {
                    idsrvApp.UseIdentityServer(new IdentityServerOptions
                    {
                        SigningCertificate = Startup.LoadCertificate(),
                        Factory = factory,
                        AuthenticationOptions = new AuthenticationOptions
                        {
                            EnableLocalLogin = false,
                            IdentityProviders = this.ConfigureIdentityProviders,

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
                                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                            },
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
                    IdentityProviders = this.ConfigureIdentityProviders,
                },
            };

            app.UseIdentityServer(options);
        }

        /// <summary>
        /// Сконфирировать провайдеры для windows-аутентификации (ws-federation).
        /// Метод передаётся как callback в <see cref="IdentityServerOptions.AuthenticationOptions"/>, поле
        /// IdentityProviders и вызывается самим identity server.
        /// </summary>
        /// <param name="app">Owin-приложение</param>
        /// <param name="signInAsType">Используется внутри identity server</param>
        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var options = new WsFederationAuthenticationOptions
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
                    },
                },
            };
            app.UseWsFederationAuthentication(options);
        }
    }
}
