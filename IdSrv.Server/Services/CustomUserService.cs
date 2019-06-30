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
    using IdSrv.Server.Loggers.Abstractions;
    using System.Collections.Generic;

    internal class CustomUserService : UserServiceBase
    {
        private IUserRepository UserRepository { get; set; }
        private IAuthLogger Logger { get; set; }

        public CustomUserService(IUserRepository userRepository, IAuthLogger logger = null)
        {
            this.UserRepository = userRepository;
            this.Logger = logger;
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            IdSrvUserDTO user = await this.UserRepository.GetUserByUserNameAndPasswordAsync(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult(errorMessage: $"User \"{user.UserName}\" is blocked") :
                    new AuthenticateResult(user.Id.ToString(), user.UserName);
                await this.Logger?.UserSignedInAsync(
                    userId: user.Id.ToString(), 
                    userName: user.UserName, 
                    clientId: context.SignInMessage.ClientId, 
                    isBlocked: user.IsBlocked);
            }
            else
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: context.UserName,
                    clientId: context.SignInMessage.ClientId);
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
            await this.Logger?.ProfileDataAccessedAsync(
                userId: user?.Id.ToString(),
                userName: user?.UserName,
                clientId: context.Client?.ClientId,
                clientName: context.Client?.ClientName,
                isBlocked: user != null ? user.IsBlocked : false);

            if (user != null)
            {
                if (!user.IsBlocked)
                {
                    context.IssuedClaims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                        new Claim(Constants.ClaimTypes.Name, user.UserName.ToString())
                    };
                }
            }
        }

        public override async Task SignOutAsync(SignOutContext context)
        {
            string userId = context.Subject?.Claims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject)?.Value;
            if (userId != null && context.ClientId != null)
            {
                await this.Logger?.UserSignedOutAsync(
                    userId: userId,
                    userName: context.Subject?.Claims?.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Name)?.Value,
                    clientId: context.ClientId);
            }
        }
    }
}