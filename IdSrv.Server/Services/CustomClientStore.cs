﻿namespace IdSrv.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Хранилище клиентов identity server. Класс необходим для расширения поведения identity server
    /// для того, чтобы он получал клиентов из собственного репозитория клиентов.
    /// Реализация <see cref="IClientStore"/> - интерфейса identity server для получения
    /// информации о клиентах.
    /// </summary>
    internal class CustomClientStore : IClientStore
    {
        /// <summary>
        /// Инициализирует хранилище клиентов.
        /// </summary>
        /// <param name="clientRepository">
        /// Репозиторий клиентов (например, rest-клиент).
        /// </param>
        /// <param name="scopes">
        /// Набор scope-ов, который назначается всем клиентам.
        /// <see cref="Scope"/> - класс как часть api identity server, за информацией по ним
        /// см. документацию.
        /// </param>
        /// <param name="isWindowsAuth">
        /// Используется ли хранилище для работы с windows-клиентами (wpf, winforms).
        /// Один объект хранилища может работать либо только с не windows-клиентам (то есть с сайтам),
        /// либо только с windows-клинетами (то есть с приложениями).
        /// </param>
        public CustomClientStore(IClientRepository clientRepository, IEnumerable<Scope> scopes, bool isWindowsAuth = false)
        {
            this.IsWindowsAuth = isWindowsAuth;
            this.ClientRepository = clientRepository;
            this.Scopes = scopes;
        }

        /// <summary>
        /// Получает или задает значение, показывающее, работает ли хранилище с windows-клиентами.
        /// Если выставлено в true, то хранилище может работать только с windows-клиентами.
        /// Если выставлено в false, то хранилище может работать только с не windows-клиентами, 
        /// то есть только с сайтами.
        /// </summary>
        private bool IsWindowsAuth { get; set; }

        /// <summary>
        /// Получает или задает репозиторий клиентов, через который хранилище получает
        /// информацию о клиентах.
        /// </summary>
        private IClientRepository ClientRepository { get; set; }

        /// <summary>
        /// Получает или задает набор scope-ов - объектов типа <see cref="Scope"/>.
        /// Данные scope-ы будут назначаться всем клиентам.
        /// </summary>
        private IEnumerable<Scope> Scopes { get; set; }

        /// <inheritdoc/>
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            IdSrvClientDto clientFromRepo = await this.ClientRepository.GetClientByIdAsync(clientId);

            if (clientFromRepo == null)
            {
                return null;
            }

            var client = new Client
            {
                Enabled = !clientFromRepo.IsBlocked,
                ClientName = clientFromRepo.Name,

                // ID клиента, также указывается в настройках самого клиента, см. конфигарцию в классе Startup
                // проектов Site1 и Site2
                ClientId = clientFromRepo.Id.ToString(),

                ClientSecrets = new List<Secret>
                {
                    new Secret(clientFromRepo.Secret.Sha256()),
                },

                Flow = Flows.ResourceOwner,

                // Время жизни identity токена в секундах, то есть токена идентификации.
                // Также есть access токен, то есть токен для доступа к данным,
                // он обычно имеет короткое время жизни.
                // Для того, чтобы сервер аутентификации "забывал" логин и пароль пользователя, 
                // надо также выставить время жизни cookie сессии сервера аутентификации, см.
                // класс Startup.
                IdentityTokenLifetime = 5 * 60,

                AccessTokenType = AccessTokenType.Reference,

                // При входе на сайте через сервер аутентификации в первый раз у пользователя
                // спрашивают, какие данные сайт может использовать (например, может ли сайт просматривать
                // профиль пользователя). Для текущих целей не нужно, поэтому пропускаем.
                RequireConsent = false,

                AllowedScopes = this.Scopes.Select(s => s.Name).ToList(),

                // Scope-ы в данном примере не освещаются, по идее с помощью них  можно разделить
                // клиентов (сайты) на области и рулить ими по-отдельности, допускать пользователей
                // в разные области. Для текущих целей пока не нужно.
                // AllowAccessToAllScopes = true,
            };

            // Если строка с uri пустая, значит это wpf-клиент (или нечто подобное, то есть не сайт)
            // Поэтому ставим другой Flow, и добавляем редиректы 
            // (конечно, это можно сделать красивее, но для демонстрации оставил так)
            if (clientFromRepo.Uri != null)
            {
                client.Flow = Flows.Hybrid;

                // Адрес сайта, куда будет редиректить после входа (по идее должен совпадать с адресом
                // самого сайта)
                client.RedirectUris = new List<string> { clientFromRepo.Uri };

                // Адрес, на который редиректит после выхода
                client.PostLogoutRedirectUris = (await this.ClientRepository.GetAllUrisAsync()).ToList();
            }
            else if (this.IsWindowsAuth)
            {
                // Если это wpf-клиент и при этом используется windows аутентификация, то необходимо изменить некоторые параметры
                client.Flow = Flows.Custom;
                client.AllowedCustomGrantTypes = new List<string> { "winauth" };
            }

            return client;
        }
    }
}