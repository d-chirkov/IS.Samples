﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IS.Repos;

namespace IS
{
    public class DbUserService : UserServiceBase
    {
        IUserRepo repo;

        public DbUserService(IUserRepo repo)
        {
            this.repo = repo;
        }

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = repo.GetUser(context.UserName, context.Password);
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id, user.Name);
            }

            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = repo.GetUser(Convert.ToInt32(context.Subject.GetSubjectId()));
            if (user != null && context.RequestedClaimTypes.Contains(Constants.ClaimTypes.Name))
            {
                // Единственный claim который есть у пользователя в рамках примера - его username
                context.IssuedClaims = new[] { new Claim(Constants.ClaimTypes.Name, user.Name) };
            }

            return Task.FromResult(0);
        }
    }
}