namespace IdSrv.Server.Services
{
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.Models;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class RestUserService : UserServiceBase
    {
        private HttpClient HttpClient { get; set; }

        public RestUserService(string restServiceUri)
        {
            this.HttpClient = new HttpClient();
            this.HttpClient.BaseAddress = new Uri(restServiceUri);
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = await this.GetUserByUserNameAndPasswordAsync(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id.ToString(), user.UserName);
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDTO user = await this.GetUserByIdAsync(context.Subject.GetSubjectId());
            if (user != null && context.RequestedClaimTypes.Contains(Constants.ClaimTypes.Name))
            {
                // Единственный claim который есть у пользователя в рамках примера - его username
                context.IssuedClaims = new[] { new Claim(Constants.ClaimTypes.Name, user.UserName) };
            }
        }

        private async Task<IdSrvUserDTO> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            var authInfo = new IdSrvUserAuthDTO { UserName = userName, Password = password };
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync("GetByAuthInfo", authInfo);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsAsync<IdSrvUserDTO>() : null;
        }

        private async Task<IdSrvUserDTO> GetUserByIdAsync(string id)
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