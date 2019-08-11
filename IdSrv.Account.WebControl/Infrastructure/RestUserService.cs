namespace IdSrv.Account.WebControl.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.RestClient;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Реализация <see cref="IUserService"/> как HTTP Rest-клиента.
    /// Класс просто оборачивает поведение сгенерированного nswag-ом клиента,
    /// чтобы контроллеры не зависели от автоматически генерируемого кода.
    /// </summary>
    public class RestUserService : IUserService
    {
        /// <summary>
        /// Инициализирует сервис для работы с пользователями в WebApi.
        /// </summary>
        /// <param name="restClient">
        /// Сгенерированный nswag-ом rest клиент.
        /// </param>
        public RestUserService(IUserRestClient restClient)
        {
            this.RestClient = restClient;
        }

        /// <summary>
        /// Получает или задает rest-клиента для WebApi,
        /// предоставляющего доступ к пользователям identity server.
        /// </summary>
        private IUserRestClient RestClient { get; set; }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть null только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<IEnumerable<IdSrvUserDto>> GetUsersAsync()
        {
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAllAsync());
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDto passwords)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.ChangePasswordAsync(passwords));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> CreateUserAsync(NewIdSrvUserDto newUser)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.CreateAsync(newUser));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.DeleteAsync(id));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// В случае ошибки подключения к WebApi возникнет системное исключение,
        /// внутри метода оно никак не перехватывается. Метод может вернуть false только
        /// в случае успешного подключения (когда ответ WebApi интепретируется как ошибка).
        /// </remarks>
        public async Task<bool> ChangeBlock(IdSrvUserBlockDto block)
        {
            return await RestApiHelpers.CallBoolApi(() => this.RestClient.ChangeBlockingAsync(block));
        }
    }
}