using System;
using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Configuration;
using Microsoft.Owin;
using Owin;
using IS.Models;
using IS.Repos;
using IdentityServer3.Core.Services;

// Сервер аутентификации является OWIN-based, добавляем ссылку
[assembly: OwinStartup(typeof(IS.Startup))]

namespace IS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Настраиваем сервер аутентификации
            app.Map("/identity", idsrvApp =>
            {
                var factory = new IdentityServerServiceFactory().UseInMemoryScopes(StandardScopes.All);

                var clientStore = new DbClientStore(new ClientRepo($"{AppDomain.CurrentDomain.BaseDirectory}is.sqlite", "clients"));
                factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);

                var userService = new DbUserService(new UserRepo($"{AppDomain.CurrentDomain.BaseDirectory}is.sqlite", "users"));
                factory.UserService = new Registration<IUserService>(resolver => userService);

                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    // Имя сервера аутентификации, можно будет наблюдать вверху страницы
                    SiteName = "Embedded IdentityServer",
                    
                    // Загружаем сертификат
                    SigningCertificate = LoadCertificate(),

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
                            ExpireTimeSpan = TimeSpan.FromMinutes(5)
                        }
                    },
                });
            });
        }

        X509Certificate2 LoadCertificate()
        {
            // Тестовый сертификат, взят с сайта identityserver3, можно ставить свой. 
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}