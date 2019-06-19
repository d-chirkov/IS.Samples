namespace IdSrv.Server.Services
{
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdSrv.Account.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;

    public class RestClientStore : IClientStore
    {
        private bool IsWindowsAuth { get; set; }

        private HttpClient HttpClient { get; set; }

        public RestClientStore(string restServiceUri, bool isWindowsAuth = false)
        {
            this.IsWindowsAuth = isWindowsAuth;
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            IdSrvClientDTO clientFromRest = await this.GetClientByIdAsync(clientId);
            if (clientFromRest == null)
            {
                return null;
            }

            var client = new Client
            {
                Enabled = true,
                ClientName = clientFromRest.Name,

                // ID клиента, также указывается в настройках самого клиента, см. конфигарцию в классе Startup
                // проектов Site1 и Site2
                ClientId = clientFromRest.Id.ToString(),

                ClientSecrets = new List<Secret>
                {
                    new Secret(clientFromRest.Secret.Sha256()),
                },

                Flow = Flows.ResourceOwner,

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

                // Scope-ы в данном примере не освещаются, по идее с помощью них можно разделить 
                // клиентов (сайты) на области и рудить ими по-отдельности, допускать пользователей
                // в разные области. Для текущих целей пока не нужно.
                AllowAccessToAllScopes = true
            };

            // Если строка с uri пустая, значит это wpf-клиент (или нечто подобное, то есть не сайт)
            // Поэтому ставим другой Flow, и добавляем редиректы 
            // (конечно, это можно сделать красивее, но для демонстрации оставил так)
            if (clientFromRest.Uri != null)
            {
                client.Flow = Flows.Hybrid;
                // Адрес сайта, куда будет редиректить после входа (по идее должен совпадать с адресом
                // самого сайта)
                client.RedirectUris = new List<string> { clientFromRest.Uri };
                // Адрес, на который редиректит после выхода
                client.PostLogoutRedirectUris = (await this.GetAllUrisAsync()).ToList();
            }
            // Если это wpf-клиент и при этом используется windows аутентификация, то необходимо изменить некоторые параметры
            else if (this.IsWindowsAuth)
            {
                client.Flow = Flows.Custom;
                client.AllowedCustomGrantTypes = new List<string> { "winauth" };
            }

            return client;
        }

        private async Task<IdSrvClientDTO> GetClientByIdAsync(string clientId)
        {
            if (!Guid.TryParse(clientId, out Guid result))
            {
                return null;
            }
            HttpResponseMessage response = await this.HttpClient.GetAsync(clientId);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvClientDTO>() : null;
        }

        private async Task<IEnumerable<string>> GetAllUrisAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAllUris");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<string>>() : null;
        }
    }
}