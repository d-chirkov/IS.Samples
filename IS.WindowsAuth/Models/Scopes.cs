using System.Collections.Generic;
using System.Linq;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace IS.WindowsAuth.Models
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return StandardScopes.All.Concat(new[]
            {
                new Scope
                {
                    Name = "idmgr",
                    DisplayName = "IdentityManager",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                    ShowInDiscoveryDocument = false,

                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Name),
                        new ScopeClaim(Constants.ClaimTypes.Role)
                    }
                }
            });
        }
    }
}