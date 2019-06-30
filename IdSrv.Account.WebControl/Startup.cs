using System;
using System.Threading.Tasks;
using IdSrv.AspNet.Helpers;
using IdSrv.Connector;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(IdSrv.Account.WebControl.Startup))]

namespace IdSrv.Account.WebControl
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            string idsrvAddress = "https://localhost:44363/identity";
            app
                .UseAuthServer(idsrvAddress)
                .WithClientId("d0864ea8-fa8b-4c85-8e71-f5af0097de92")
                .WithClientSecret("web-control-secret")
                .WithOwnAddress("https://localhost:44312/");

            IdSrvConnection.IdSrvAddress = idsrvAddress;
            IdSrvConnection.UseAutoLogoutWhenNoAccess = true;
        }
    }
}
