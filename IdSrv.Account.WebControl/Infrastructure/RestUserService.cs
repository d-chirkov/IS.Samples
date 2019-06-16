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
        public HttpClient Client { get; set; }

        public RestUserService(string restServiceUri)
        {
            this.Client = new HttpClient();
            this.Client.BaseAddress = new Uri(restServiceUri);
            this.Client.DefaultRequestHeaders.Accept.Clear();
            this.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<IdSrvUserDTO>> GetUsersAsync()
        {
            IEnumerable<IdSrvUserDTO> users = null;
            HttpResponseMessage response = await this.Client.GetAsync("api/User/GetAll");
            if (response.IsSuccessStatusCode)
            {
                users = await response.Content.ReadAsAsync<IEnumerable<IdSrvUserDTO>>();
            }
            return users;
        }

        public async Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDTO passwords)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("api/User/ChangePassword", passwords);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateUserAsync(NewIdSrvUserDTO newUser)
        {
            HttpResponseMessage response = await this.Client.PutAsJsonAsync("api/User", newUser);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            HttpResponseMessage response = await this.Client.DeleteAsync($"api/User/{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeBlock(IdSrvUserBlockDTO block)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("api/User/ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}