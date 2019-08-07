namespace IdSrv.Account.WebControl.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Реализация <see cref="IClientService"/> как HTTP Rest-клиента.
    /// </summary>
    public class RestClientService : IClientService
    {
        /// <summary>
        /// Инициализирует rest-клиента для работы с конкретным WebApi.
        /// </summary>
        /// <param name="restServiceUri">
        /// URI-адрес сервиса WebApi, предоставляющего доступ к клиентам identity server.
        /// </param>
        public RestClientService(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Получает или задает HTTP-клиента (подключение) к WebApi,
        /// предоставляющего доступ к клиентам identity server.
        /// </summary>
        private HttpClient HttpClient { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть null только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<IEnumerable<IdSrvClientDto>> GetClientsAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvClientDto>>() : null;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть null только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<IdSrvClientDto> GetClientByIdAsync(Guid id)
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvClientDto>() : null;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> CreateClientAsync(NewIdSrvClientDto newClient)
        {
            HttpResponseMessage response = await this.HttpClient.PutAsJsonAsync(string.Empty, newClient);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> UpdateClientAsync(UpdateIdSrvClientDto client)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("Update", client);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> DeleteClientAsync(Guid id)
        {
            HttpResponseMessage response = await this.HttpClient.DeleteAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> ChangeBlock(IdSrvClientBlockDto block)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}