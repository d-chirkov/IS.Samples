namespace IdSrv.Server.Services
{
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Validation;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    internal class CustomTokenValidator : ICustomTokenValidator
    {
        private IClientRepository ClientRepository { get; set; }

        private IUserRepository UserRepository { get; set; }

        public CustomTokenValidator(IUserRepository userRepository, IClientRepository clientRepository)
        {
            this.UserRepository = userRepository;
            this.ClientRepository = clientRepository;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(TokenValidationResult result)
        {
            if (!await this.IsTokenValid(result))
            {
                result.IsError = true;
                result.Error = Constants.ProtectedResourceErrors.InvalidToken;
                result.Claims = null;
            }
            return result;
        }

        public async Task<TokenValidationResult> ValidateIdentityTokenAsync(TokenValidationResult result)
        {
            if (!await this.IsTokenValid(result))
            {
                result.IsError = true;
                result.Error = Constants.ProtectedResourceErrors.InvalidToken;
                result.Claims = null;
            }
            return result;
        }

        private async Task<bool> IsTokenValid(TokenValidationResult result)
        {
            if (result.ReferenceToken.SubjectId == null || result.ReferenceToken.ClientId == null)
            {
                return false;
            }
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(result.ReferenceToken.SubjectId);
            if (user == null || user.IsBlocked)
            {
                return false;
            }
            IdSrvClientDTO client = await this.ClientRepository.GetClientByIdAsync(result.ReferenceToken.ClientId);
            if (client == null || client.IsBlocked)
            {
                return false;
            }
            return true;
        }
    }
}