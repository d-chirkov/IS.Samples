namespace IdSrv.Account.WebControl.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.RestClient;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Реализация <see cref="IClientService"/> как HTTP Rest-клиента.
    /// Класс просто оборачивает поведение сгенерированного nswag-ом клиента,
    /// чтобы контроллеры не зависели от автоматически генерируемого кода.
    /// </summary>
    public class RestClientService : IClientService
    {
        /// <summary>
        /// Инициализирует сервис для работы с клиентами в WebApi.
        /// </summary>
        /// <param name="restClient">
        /// Сгенерированный nswag-ом rest клиент.
        /// </param>
        public RestClientService(IClientRestClient restClient)
        {
            this.RestClient = restClient;
        }

        /// <summary>
        /// Получает или задает rest-клиента для WebApi,
        /// предоставляющего доступ к клиентам identity server.
        /// </summary>
        private IClientRestClient RestClient { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть null только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<IEnumerable<IdSrvClientDto>> GetClientsAsync()
        {
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAllAsync());
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть null только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<IdSrvClientDto> GetClientByIdAsync(Guid id)
        {
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAsync(id));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> CreateClientAsync(NewIdSrvClientDto newClient)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.CreateAsync(newClient));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> UpdateClientAsync(UpdateIdSrvClientDto client)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.UpdateAsync(client));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> DeleteClientAsync(Guid id)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.DeleteAsync(id));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> ChangeBlock(IdSrvClientBlockDto block)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.ChangeBlockingAsync(block));
        }
    }
}