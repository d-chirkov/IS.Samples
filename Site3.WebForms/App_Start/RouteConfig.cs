namespace Site3.WebForms
{
    using System.Web.Routing;
    using Microsoft.AspNet.FriendlyUrls;

    /// <summary>
    /// Конфигурация маршрутов ASP.NET.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// Добавить маршруты в коллекцию.
        /// </summary>
        /// <param name="routes">Коллекция маршрутов, выходной параметр.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        }
    }
}
