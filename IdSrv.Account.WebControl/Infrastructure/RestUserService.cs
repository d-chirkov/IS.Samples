﻿namespace IdSrv.Account.WebControl.Infrastructure
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
            HttpResponseMessage response = await this.Client.GetAsync("GetAll");
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IEnumerable<IdSrvUserDTO>>() : null;
        }

        public async Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDTO passwords)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("ChangePassword", passwords);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateUserAsync(NewIdSrvUserDTO newUser)
        {
            HttpResponseMessage response = await this.Client.PutAsJsonAsync("", newUser);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            HttpResponseMessage response = await this.Client.DeleteAsync($"{id.ToString()}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeBlock(IdSrvUserBlockDTO block)
        {
            HttpResponseMessage response = await this.Client.PostAsJsonAsync("ChangeBlocking", block);
            return response.IsSuccessStatusCode;
        }
    }
}