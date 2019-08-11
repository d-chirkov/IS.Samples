namespace IdSrv.Server.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.RestClient;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Реализация репозитория в виде rest-клиента к WebApi, предоставляющего доступ к данным о клиентах.
    /// </summary>
    internal class RestClientRepository : IClientRepository
    {
        /// <summary>
        /// Инициализирует репозиторий для работы с клиентами в WebApi.
        /// </summary>
        /// <param name="restClient">
        /// Сгенерированный nswag-ом rest клиент.
        /// </param>
        public RestClientRepository(IClientRestClient restClient)
        {
            this.RestClient = restClient;
        }

        /// <summary>
        /// Получает или задает rest-клиента для WebApi,
        /// предоставляющего доступ к клиентам identity server.
        /// </summary>
        private IClientRestClient RestClient { get; set; }

        /// <inheritdoc/>
        public async Task<IdSrvClientDto> GetClientByIdAsync(string clientId)
        {
            if (Guid.TryParse(clientId, out Guid result))
            {
                return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAsync(result));
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAllUrisAsync()
        {
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAllUrisAsync());
        }
    }
}