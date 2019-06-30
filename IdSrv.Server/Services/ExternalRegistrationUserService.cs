﻿namespace IdSrv.Server.Services
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
    using IdSrv.Server.Repositories.Abstractions;

    internal class ExternalRegistrationUserService : UserServiceBase
    {
        public IUserRepository UserRepository { get; set; }

        public ExternalRegistrationUserService(IUserRepository userRepository)
        {
            this.UserRepository = userRepository;
        }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            string userName = context.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name)?.Value;
            if (userName == null)
            {
                return;
            }
            IdSrvUserDTO user = await this.UserRepository.GetUserByUserNameAsync(userName);
            if (user != null && !user.IsBlocked)
            {
                context.AuthenticateResult = new AuthenticateResult(
                    user.Id.ToString(), 
                    userName, 
                    identityProvider: context.ExternalIdentity.Provider);
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult($"User is \"{userName}\" blocked");
            }
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            IdSrvUserDTO user = await this.UserRepository.GetUserByIdAsync(context.Subject.GetSubjectId());
            if (user != null && !user.IsBlocked)
            {
                var userClaims = new List<Claim>
                {
                    new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                    new Claim(Constants.ClaimTypes.Name, user.UserName.ToString())
                };
                context.IssuedClaims = userClaims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }
        }
    }
}