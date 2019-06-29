// ВАЖНО: в зависимостях стоит версия IdentityModel 2.6.0 - далеко не самая свежая, но для .Net Framework 4.5.1 новее нет
// (IdentityModel нужен для добавления секрета приложения-клиента)

using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

// Конфигурация происходит в классе Startup, фактически добавляется middleware, так что добавляем ссылку на owin
[assembly: OwinStartup(typeof(Site1.Startup))]

namespace Site1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app
                .UseAuthServer("https://localhost:44363/identity")
                .WithClientId("1bf49861-75b8-4069-a6e8-3f2fd8a7e15e")
                .WithClientSecret("secret")
                .WithOwnAddress("http://localhost:51542/");

            // Добавляем пользователей в UserAccessControl, добавленные пользователи смогут заходить на сайт.
            // Для теста добавим двух (bob:123, alice:1234)
            UserAccessControl.AddAccessTo("ea0c356f-512e-4442-bcb7-054a5c4bde20", "d1eed09f-e03d-41a9-9dbb-8abac9294db2");
        }
    }
}