namespace IdSrv.Account.WebControl
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using Autofac;
    using Autofac.Integration.Mvc;
    using IdSrv.Account.WebControl.Infrastructure;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Главный класс приложения, содержит точку входа сервиса.
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Точка входа в приложение, вызывается сервером IIS при старте сервиса.
        /// </summary>
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();
            builder
                .Register(v => new RestUserService("https://localhost:44397/Api/User/"))
                .As<IUserService>()
                .InstancePerRequest();
            builder
                .Register(v => new RestClientService("https://localhost:44397/Api/Client/"))
                .As<IClientService>()
                .InstancePerRequest();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
