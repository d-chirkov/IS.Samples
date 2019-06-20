﻿namespace IdSrv.Server.Repositories
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    internal class RestUserRepository : IUserRepository
    {
        private HttpClient HttpClient { get; set; }

        public RestUserRepository(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IdSrvUserDTO> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            var authInfo = new IdSrvUserAuthDTO { UserName = userName, Password = password };
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("GetByAuthInfo", authInfo);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDTO>() : null;
        }

        public async Task<IdSrvUserDTO> GetUserByIdAsync(string id)
        {
            if (Guid.TryParse(id, out Guid result))
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync($"{result.ToString()}");
                return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDTO>() : null;
            }

            return null;
        }
    }
}