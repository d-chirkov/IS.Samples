using System;
using System.Threading.Tasks;
using IdSrv.AspNet.Helpers;
using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Site2.Mvc5.Startup))]

namespace Site2.Mvc5
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string idsrvAddress = "https://localhost:44363/winidentity";
            app
               .UseAuthServer(idsrvAddress)
               .WithClientId("2ea7fd35-1fd7-4782-832e-6edc9edad8ed")
               .WithClientSecret("secret")
               .WithOwnAddress("http://localhost:51573/");

            IdSrvConnection.IdSrvAddress = idsrvAddress;
            IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
        }
    }
}
