namespace IdSrv.Server.Services
{
    using System.Threading.Tasks;
    using System.Security.Claims;
    using IdentityServer3.Core.Services;
    using IdSrv.Server.Repositories.Abstractions;
    using IdSrv.Account.Models;

    internal class CustomAuthenticationSessionValidator : IAuthenticationSessionValidator
    {
        private IUserRepository UserRepository { get; set; }

        public CustomAuthenticationSessionValidator(IUserRepository userRepository)
        {
            this.UserRepository = userRepository;
        }

        public async Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject)
        {
            string userId = subject.FindFirst("sub")?.Value;
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(userId);
            return user != null && !user.IsBlocked;
        }
    }
}