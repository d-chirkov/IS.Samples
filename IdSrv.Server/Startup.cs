using Microsoft.Owin;

[assembly: OwinStartup(typeof(IdSrv.Server.Startup))]

namespace IdSrv.Server
{
    using Owin;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;
    using Services;
    using Serilog;
    using IdSrv.Server.Repositories;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Настраиваем сервер аутентификации
            app.Map("/identity", idsrvApp =>
            {
                var factory = new IdentityServerServiceFactory().UseInMemoryScopes(StandardScopes.All);

                var clientRepository = new RestClientRepository("https://localhost:44397/Api/Client/");
                var clientStore = new CustomClientStore(clientRepository);
                factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);

                var userRepository = new RestUserRepository("https://localhost:44397/Api/User/");
                var userService = new CustomUserService(userRepository);
                factory.UserService = new Registration<IUserService>(resolver => userService);

                var sessionValidator = new CustomAuthenticationSessionValidator(userRepository);
                factory.AuthenticationSessionValidator = new Registration<IAuthenticationSessionValidator>(resolver => sessionValidator);

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

                    // Загружаем сертификат
                    SigningCertificate = this.LoadCertificate(),

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
                    },
                });
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}\\IS.Log.txt")
                .CreateLogger();
        }

        X509Certificate2 LoadCertificate()
        {
            // Тестовый сертификат, взят с сайта identityserver3, можно ставить свой. 
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
