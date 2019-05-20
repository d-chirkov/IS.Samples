﻿namespace SharedLib.IS.IdSrvImpls
{
    using IdentityServer3.Core;
    using IdentityServer3.Core.Extensions;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services.Default;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class ISUserService : UserServiceBase
    {
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = UserRepo.GetUser(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id, user.Name);
            }

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = UserRepo.GetUserById(context.Subject.GetSubjectId());
            if (user != null && context.RequestedClaimTypes.Contains(Constants.ClaimTypes.Name))
            {
                // Единственный claim который есть у пользователя в рамках примера - его username
                context.IssuedClaims = new[] { new Claim(Constants.ClaimTypes.Name, user.Name) };
            }

            return Task.FromResult(0);
        }
    }
}