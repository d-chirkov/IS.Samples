using System.Web.Http;
using IdSrv.Account.WebApi;
using Swashbuckle.Application;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace IdSrv.Account.WebApi
{
    /// <summary>
    /// Конфигурация дла swagger.
    /// </summary>
    public class SwaggerConfig
    {
        /// <summary>
        /// Регистрирует swagger.
        /// </summary>
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c => c.SingleApiVersion("v1", "IdSrv.Account.WebApi"))
                .EnableSwaggerUi();
        }
    }
}
