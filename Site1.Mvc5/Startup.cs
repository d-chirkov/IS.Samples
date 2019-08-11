using IdSrv.AspNet.Helpers;
using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Site1.Mvc5.Startup))]

namespace Site1.Mvc5
{
    /// <summary>
    /// Класс для настройки Owin.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Сконфигурировать приложение Owin.
        /// </summary>
        /// <param name="app">Сборщик приложения.</param>
        public void Configuration(IAppBuilder app)
        {
            string idsrvAddress = "https://localhost:44363/identity";
            app
                .UseAuthServer(idsrvAddress)
                .WithClientId("78f36f55-784d-4895-8913-f9a76b807a5c")
                .WithClientSecret("123")
                .WithOwnAddress("https://localhost:44393/");

            IdSrvConnection.IdSrvAddress = idsrvAddress;
            IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
        }
    }
}