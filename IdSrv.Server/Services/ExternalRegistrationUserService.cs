namespace IdSrv.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using IdSrv.Account.Models;
    using IdSrv.Server.Loggers.Abstractions;
    using IdSrv.Server.Repositories.Abstractions;

    internal class ExternalRegistrationUserService : UserServiceBase
    {
        public ExternalRegistrationUserService(IUserRepository userRepository, IAuthLogger logger = null)
        {
            this.UserRepository = userRepository;
            this.Logger = logger;
        }

        public IUserRepository UserRepository { get; set; }

        private IAuthLogger Logger { get; set; }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            string userName = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                await this.Logger?.UnsuccessfulSigningInAsync(
                    userName: userName,
                    clientId: context.SignInMessage.ClientId);
                return;
            }

            IdSrvUserDto user = await this.UserRepository.GetUserByUserNameAsync(userName);
            if (user != null && !user.IsBlocked)
            {
                context.AuthenticateResult = user.IsBlocked ?
                    new AuthenticateResult($"User is \"{userName}\" blocked") :
                    new AuthenticateResult(
                        user.Id.ToString(),
                        userName,
                        identityProvider: context.ExternalIdentity.Provider);

                await this.Logger?.UserSignedInAsync(
                    userId: user.Id.ToString(),
                    clientId: context.SignInMessage.ClientId,
                    userName: user.UserName,
                    isBlocked: user.IsBlocked);
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult($"User is \"{userName}\" not registered");
                await this.Logger?.NotRegisteredUserTryToSignInAsync(
                    userName: user.UserName,
                    clientId: context.SignInMessage.ClientId);
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDto user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
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
                        new Claim(Constants.ClaimTypes.Name, user.UserName.ToString()),
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