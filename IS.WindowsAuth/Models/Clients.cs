using IdentityServer3.Core.Models;
using System.Collections.Generic;

namespace IS.WindowsAuth.Models
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,

                    ClientName = "Site1",
                    
                    ClientId = "site1",

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256()),
                    },
                    
                    Flow = Flows.Hybrid,
                    
                    IdentityTokenLifetime = 30,
                    
                    RequireConsent = false,
                    
                    RedirectUris = new List<string>
                    {
                        "http://localhost:51542/"
                    },
                    
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:51542/"
                    },
                    
                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,

                    ClientName = "SiteN",

                    ClientId = "78f36f55-784d-4895-8913-f9a76b807a5c",

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("123".Sha256()),
                    },

                    Flow = Flows.Hybrid,

                    IdentityTokenLifetime = 30,

                    RequireConsent = false,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:57161/"
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:57161/"
                    },

                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,

                    ClientName = "Site1",
                    
                    ClientId = "site2",

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret2".Sha256()),

                    },
                    
                    Flow = Flows.Hybrid,
                    
                    IdentityTokenLifetime = 30,
                    
                    RequireConsent = false,
                    
                    RedirectUris = new List<string>
                    {
                        "http://localhost:51566/"
                    },
                    
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:51566/"
                    },
                    
                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,

                    ClientName = "Site3",
                    
                    ClientId = "site3",

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret3".Sha256()),

                    },
                    
                    Flow = Flows.Hybrid,
                    
                    IdentityTokenLifetime = 30,
                    
                    RequireConsent = false,
                    
                    RedirectUris = new List<string>
                    {
                        "http://localhost:56140/"
                    },
                    
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:56140/"
                    },

                    AllowAccessToAllScopes = true
                },

                new Client
                {
                    Enabled = true,

                    ClientName = "Desktop",

                    ClientId = "9bf3b01c-9689-4db2-87a2-09fd390df550",

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret3".Sha256()),
                    },

                    Flow = Flows.Custom,

                    IdentityTokenLifetime = 30,

                    RequireConsent = false,

                    AllowAccessToAllScopes = true,

                    AllowedCustomGrantTypes = new List<string> { "winauth" }
                }
            };
        }
    }
}