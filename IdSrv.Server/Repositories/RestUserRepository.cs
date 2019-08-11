namespace IdSrv.Server.Repositories
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.RestClient;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Реализация репозитория в виде rest-клиента к WebApi, предоставляющего доступ к данным о пользователях.
    /// </summary>
    internal class RestUserRepository : IUserRepository
    {
        /// <summary>
        /// Инициализирует репозиторий для работы с пользователями в WebApi.
        /// </summary>
        /// <param name="restClient">
        /// Сгенерированный nswag-ом rest клиент.
        /// </param>
        public RestUserRepository(IUserRestClient restClient)
        {
            this.RestClient = restClient;
        }

        /// <summary>
        /// Получает или задает rest-клиента для WebApi,
        /// предоставляющего доступ к пользователям identity server.
        /// </summary>
        private IUserRestClient RestClient { get; set; }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            var authInfo = new IdSrvUserAuthDto { UserName = userName, Password = password };
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetByAuthInfoAsync(authInfo));
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByIdAsync(string id)
        {
            if (Guid.TryParse(id, out Guid result))
            {
                return await RestApiHelpers.CallValueApi(() => this.RestClient.GetAsync(result));
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByUserNameAsync(string userName)
        {
            return await RestApiHelpers.CallValueApi(() => this.RestClient.GetByUserNameAsync(userName));
        }
    }
}