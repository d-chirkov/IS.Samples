namespace IdSrv.Server.Repositories
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    /// <summary>
    /// Реализация репозитория в виде rest-клиента к WebApi, предоставляющего доступ к данным о пользователях.
    /// </summary>
    internal class RestUserRepository : IUserRepository
    {
        /// <summary>
        /// Инициализирует объект. При этом создаётся и настривается объект <see cref="HttpClient"/>,
        /// но подключение не устанавливается.
        /// </summary>
        /// <param name="restServiceUri">URI-адрес WebApi.</param>
        public RestUserRepository(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Получает или задает http-клиента.
        /// </summary>
        private HttpClient HttpClient { get; set; }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            var authInfo = new IdSrvUserAuthDto { UserName = userName, Password = password };
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("GetByAuthInfo", authInfo);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDto>() : null;
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByIdAsync(string id)
        {
            if (Guid.TryParse(id, out Guid result))
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync($"{result.ToString()}");
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDto>() : null;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetUserByUserNameAsync(string userName)
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync($"GetByUserName?userName={HttpUtility.UrlEncode(userName)}");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDto>() : null;
        }
    }
}