namespace IdSrv.Server.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    internal class RestClientRepository : IClientRepository
    {
        public RestClientRepository(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private HttpClient HttpClient { get; set; }

        public async Task<IdSrvClientDto> GetClientByIdAsync(string clientId)
        {
            if (!Guid.TryParse(clientId, out Guid result))
            {
                return null;
            }

            HttpResponseMessage response = await this.HttpClient.GetAsync(clientId);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvClientDto>() : null;
        }

        public async Task<IEnumerable<string>> GetAllUrisAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAllUris");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<string>>() : null;
        }
    }
}