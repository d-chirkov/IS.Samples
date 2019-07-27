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

    public class RestUserService : IUserService
    {
        private HttpClient HttpClient { get; set; }

        public RestUserService(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<IdSrvUserDto>> GetUsersAsync()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvUserDto>>() : null;
        }

        public async Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDto passwords)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangePassword", passwords);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateUserAsync(NewIdSrvUserDto newUser)
        {
            HttpResponseMessage response = await this.HttpClient.PutAsJsonAsync("", newUser);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            HttpResponseMessage response = await this.HttpClient.DeleteAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeBlock(IdSrvUserBlockDto block)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}