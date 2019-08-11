namespace Site2.Mvc5
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Конфигурация маршрутов ASP.NET.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Добавить маршруты в коллекцию.
        /// </summary>
        /// <param name="routes">Коллекция маршрутов, выходной параметр.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
