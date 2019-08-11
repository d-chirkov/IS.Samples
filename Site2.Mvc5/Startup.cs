using IdSrv.AspNet.Helpers;
using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Site2.Mvc5.Startup))]

namespace Site2.Mvc5
{
    /// <summary>
    /// Класс для настройки Owin.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Сконфигурировать приложение Owin.
        /// </summary>
        /// <param name="app">Сборщик приложения.</param>
        public void Configuration(IAppBuilder app)
        {
            string idsrvAddress = "https://localhost:44363/winidentity";
            app
               .UseAuthServer(idsrvAddress)
               .WithClientId("2ea7fd35-1fd7-4782-832e-6edc9edad8ed")
               .WithClientSecret("secret")
               .WithOwnAddress("http://localhost:51573/");

            IdSrvConnection.IdSrvAddress = idsrvAddress;
            IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
        }
    }
}
