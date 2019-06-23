namespace IdSrv.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdSrv.Account.Models;
    using IdSrv.Server.Repositories.Abstractions;

    internal class CustomTokenHandleStore : ITokenHandleStore
    {
        private Dictionary<string, Token> Tokens;

        private IClientRepository ClientRepository { get; set; }

        private IUserRepository UserRepository { get; set; }

        public CustomTokenHandleStore(IUserRepository userRepository, IClientRepository clientRepository)
        {
            this.Tokens = new Dictionary<string, Token>();
            this.UserRepository = userRepository;
            this.ClientRepository = clientRepository;
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            var tokens = this.Tokens
                .Where(t => t.Value.SubjectId == subject && this.IsTokenValid(t.Value).Result)
                .Select(t => t.Value);
            var resultTokens = new List<ITokenMetadata>();
            foreach (var token in tokens)
            {
                if (await this.IsTokenValid(token))
                {
                    resultTokens.Add(token);
                }
            }
            return resultTokens;
        }

        public async Task<Token> GetAsync(string key)
        {
            var token = this.Tokens.Where(t => t.Key == key).FirstOrDefault().Value;
            bool valid = await this.IsTokenValid(token);
            return valid ? token : null;
        }

        public Task RemoveAsync(string key)
        {
            this.Tokens = this.Tokens
                .Where(t => t.Key != key)
                .ToDictionary(t => t.Key, t => t.Value);
            return Task.FromResult(0);
        }

        public Task RevokeAsync(string subject, string client)
        {
            this.Tokens = this.Tokens
                .Where(t => t.Value.ClientId != client && t.Value.SubjectId != subject)
                .ToDictionary(t => t.Key, t => t.Value);
            return Task.FromResult(0);
        }

        public Task StoreAsync(string key, Token value)
        {
            this.Tokens.Add(key, value);
            return Task.FromResult(0);
        }

        private async Task<bool> IsTokenValid(Token result)
        {
            if (result.SubjectId == null || result.ClientId == null)
            {
                return false;
            }
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(result.SubjectId);
            if (user == null || user.IsBlocked)
            {
                return false;
            }
            IdSrvClientDTO client = await this.ClientRepository.GetClientByIdAsync(result.ClientId);
            if (client == null || client.IsBlocked)
            {
                return false;
            }
            return true;
        }
    }
}