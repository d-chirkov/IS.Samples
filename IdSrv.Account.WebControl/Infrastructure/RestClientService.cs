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
        public HttpClient Client { get; set; }

        public RestClientService(string restServiceUri)
        {
            this.Client = new HttpClient();
            this.Client.BaseAddress = new Uri(restServiceUri);
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<IdSrvClientDTO>> GetClientsAsync()
        {
            HttpResponseMessage response = await this.Client.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvClientDTO>>() : null;
        }

        public async Task<IdSrvClientDTO> GetClientByIdAsync(Guid id)
        {
            HttpResponseMessage response = await this.Client.GetAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvClientDTO>() : null;
        }

        public async Task<bool> CreateClientAsync(NewIdSrvClientDTO newClient)
        {
            HttpResponseMessage response = await this.Client.PutAsJsonAsync("", newClient);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateClientAsync(UpdateIdSrvClientDTO client)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("Update", client);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteClientAsync(Guid id)
        {
            HttpResponseMessage response = await this.Client.DeleteAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeBlock(IdSrvClientBlockDTO block)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}