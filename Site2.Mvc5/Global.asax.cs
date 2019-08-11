namespace Site2.Mvc5
{
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    /// <summary>
    /// Главный класс приложения, содержит точку входа сервиса.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Точка входа в приложение, вызывается сервером IIS при старте сервиса.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
