namespace IdSrv.Account.WebApi
{
    using System.Web.Http;

    /// <summary>
    /// Конфигурация маршрутов WebApi.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Регистрирует маршруты к контроллерам сервиса.
        /// </summary>
        /// <param name="config">Конфигурация HttpConfiguration.</param>
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { controller = "Values", id = RouteParameter.Optional });
        }
    }
}
