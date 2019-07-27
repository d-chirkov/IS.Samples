namespace IdSrv.Account.WebControl.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    public class RestClientService : IClientService
    {
        private HttpClient HttpClient { get; set; }

        public RestClientService(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<IdSrvClientDto>> GetClientsAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvClientDto>>() : null;
        }

        public async Task<IdSrvClientDto> GetClientByIdAsync(Guid id)
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvClientDto>() : null;
        }

        public async Task<bool> CreateClientAsync(NewIdSrvClientDto newClient)
        {
            HttpResponseMessage response = await this.HttpClient.PutAsJsonAsync("", newClient);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClientAsync(UpdateIdSrvClientDto client)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("Update", client);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(Guid id)
        {
            HttpResponseMessage response = await this.HttpClient.DeleteAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeBlock(IdSrvClientBlockDto block)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}