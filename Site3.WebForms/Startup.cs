using IdSrv.AspNet.Helpers;
using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Site3.WebForms.Startup))]

namespace Site3.WebForms
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
            string idsrvAddress = "https://localhost:44363/identity";
            app
                .UseAuthServer(idsrvAddress)
                .WithClientId("2111f461-02f0-4376-96e6-7cd69796c67f")
                .WithClientSecret("secret3")
                .WithOwnAddress("http://localhost:56140/")
                .WithWebForms();

            IdSrvConnection.IdSrvAddress = idsrvAddress;
            IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
        }
    }
}
