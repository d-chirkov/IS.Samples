using System.Web.Http;
using WebActivatorEx;
using IdSrv.Account.WebApi;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace IdSrv.Account.WebApi
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c => c.SingleApiVersion("v1", "IdSrv.Account.WebApi"))
                .EnableSwaggerUi();
        }
    }
}
