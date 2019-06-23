// ВАЖНО:
// Ставим NuGet пакет System.IdentityModel.Tokens.Jwt версии 4.0.4 (это последняя из 4.x на момент написания)
// С версиями 5.x не работает.
// Остальные пакеты самые навые (на момент написания)
// 
// Делать Scopes именно таким, какой он есть тоже надо, чтобы Claim-ы пользователя подцепились
//
// Для развертывания необходимо включить Windows Authentication
// В IIS Express это ставится через Property проекты (там уже стоит Enabled, но по-умолчанию оно выключено)
// Для развёртывания на полноценном IIS - смотрим Web.config и комментарий в нём (строка 18)


using Microsoft.Owin;
using Owin;
using IdentityServer.WindowsAuthentication.Configuration;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Microsoft.Owin.Security.WsFederation;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System;
using IS.WindowsAuth.Models;
using SharedLib.IS.IdSrvImpls;
using Serilog;

[assembly: OwinStartup(typeof(IS.WindowsAuth.Startup))]

namespace IS.WindowsAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/windows", ConfigureWindowsTokenProvider);

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryScopes(Scopes.Get());

            //var clientStore = new ISClientStore(useWinAuth: true);
            factory.UseInMemoryClients(Clients.Get());
            //factory.ClientStore = new Registration<IClientStore>(resolver => clientStore);
            factory.UserService = new Registration<IUserService>(typeof(ExternalRegistrationUserService));
            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator, CustomGrantValidator>());

            app.Map("/identity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SigningCertificate = LoadCertificate(),
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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{AppDomain.CurrentDomain.BaseDirectory}\\IS.Log.txt")
                .CreateLogger();
        }

        private static void ConfigureWindowsTokenProvider(IAppBuilder app)
        {
            app.UseWindowsAuthenticationService(new WindowsAuthenticationOptions
            {
                IdpReplyUrl = "https://localhost:44384/was",
                SigningCertificate = LoadCertificate(),
                EnableOAuth2Endpoint = false
            });
        }

        private static X509Certificate2 LoadCertificate()
        {
            // Тестовый сертификат, взят с сайта identityserver3, можно ставить свой. 
            return new X509Certificate2(
                string.Format(@"{0}\bin\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var wsFederation = new WsFederationAuthenticationOptions
            {
                AuthenticationType = "windows",
                Caption = "Windows",
                SignInAsAuthenticationType = signInAsType,

                MetadataAddress = "https://localhost:44384/windows",
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
    }
}
