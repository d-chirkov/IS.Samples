using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using IS.Repos;

namespace IS
{
    public class DbClientStore : IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var clientFromDb = ClientRepo.GetClient(clientId);
            if (clientFromDb == null)
            {
                return Task.FromResult<Client>(null);
            }
            var client = new Client
            {
                Enabled = true,
                ClientName = clientFromDb.Name,

                // ID клиента, также указывается в настройках самого клиента, см. конфигарцию в классе Startup
                // проектов Site1 и Site2
                ClientId = clientFromDb.Id,

                ClientSecrets = new List<Secret>
                {
                    new Secret(clientFromDb.Secret.Sha256()),
                },

                Flow = Flows.Hybrid,

                // Время жизни identity токена в секундах, то есть токена идентификации.
                // Также есть access токен, то есть токен для доступа к данным,
                // он обычно имеет короткое время жизни.
                // Для того, чтобы сервер аутентификации "забывал" логин и пароль пользователя, 
                // надо также выставить время жизни cookie сессии сервера аутентификации, см.
                // класс Startup.
                IdentityTokenLifetime = 5 * 60,

                // При входе на сайте через сервер аутентификации в первый раз у пользователя
                // спрашивают, какие данные сайт может использовать (например, может ли сайт просматривать
                // профиль пользователя). Для текущих целей не нужно, поэтому пропускаем.
                RequireConsent = false,

                // Адрес сайта, куда будет редиректить после входа (по идее должен совпадать с адресом
                // самого сайта)
                RedirectUris = new List<string> { clientFromDb.Uri },

                // Адрес, на который редиректит после выхода
                PostLogoutRedirectUris = new List<string> { clientFromDb.Uri },

                // Scope-ы в данном примере не освещаются, по идее с помощью них можно разделить 
                // клиентов (сайты) на области и рудить ими по-отдельности, допускать пользователей
                // в разные области. Для текущих целей пока не нужно.
                AllowAccessToAllScopes = true
            };

            return Task.FromResult(client);
        }
    }
}