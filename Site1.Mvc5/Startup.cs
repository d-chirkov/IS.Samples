// ВАЖНО: в зависимостях стоит версия IdentityModel 2.6.0 - далеко не самая свежая, но для .Net Framework 4.5.1 новее нет
// (IdentityModel нужен для добавления секрета приложения-клиента)

using Microsoft.Owin;
using Owin;
using IdSrv.Connector;

// Конфигурация происходит в классе Startup, фактически добавляется middleware, так что добавляем ссылку на owin
[assembly: OwinStartup(typeof(Site1.Mvc5.Startup))]

namespace Site1.Mvc5
{
    public static class OidcClaimTypes
    {
        public const string Subject = "sub";
        public const string Name = "name";
    }

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app
                .UseAuthServer("https://localhost:44363/identity")
                .WithClientId("78f36f55-784d-4895-8913-f9a76b807a5c")
                .WithClientSecret("123")
                .WithOwnAddress("http://localhost:57161/");
        }
    }
}