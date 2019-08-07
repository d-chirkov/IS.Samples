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
    /// Реализация <see cref="IUserService"/> как HTTP Rest-клиента.
    /// </summary>
    public class RestUserService : IUserService
    {
        /// <summary>
        /// Инициализирует rest-клиента для работы с конкретным WebApi.
        /// </summary>
        /// <param name="restServiceUri">
        /// URI-адрес сервиса WebApi, предоставляющего доступ к пользвателям identity server.
        /// </param>
        public RestUserService(string restServiceUri)
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
        public async Task<IEnumerable<IdSrvUserDto>> GetUsersAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvUserDto>>() : null;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDto passwords)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangePassword", passwords);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> CreateUserAsync(NewIdSrvUserDto newUser)
        {
            HttpResponseMessage response = await this.HttpClient.PutAsJsonAsync(string.Empty, newUser);
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исклюение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> DeleteUserAsync(Guid id)
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
        public async Task<bool> ChangeBlock(IdSrvUserBlockDto block)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}