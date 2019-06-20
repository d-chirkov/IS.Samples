namespace IdSrv.Server.Services
{
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.Models;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdSrv.Server.Repositories.Abstractions;

    internal class CustomUserService : UserServiceBase
    {
        private IUserRepository UserRepository { get; set; }

        public CustomUserService(IUserRepository userRepository)
        {
            this.UserRepository = userRepository;
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            IdSrvUserDTO user = await this.UserRepository.GetUserByUserNameAndPasswordAsync(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult(errorMessage: $"User \"{user.UserName}\" is blocked") :
                    new AuthenticateResult(user.Id.ToString(), user.UserName);
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
            if (user != null && context.RequestedClaimTypes.Contains(Constants.ClaimTypes.Name))
            {
                // Единственный claim который есть у пользователя в рамках примера - его username
                context.IssuedClaims = new[] { new Claim(Constants.ClaimTypes.Name, user.UserName) };
            }
        }
    }
}