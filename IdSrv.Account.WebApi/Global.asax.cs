namespace IdSrv.Account.WebApi
{
    using System;
    using System.IO;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Web.Http;
    using Autofac;
    using Autofac.Integration.WebApi;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();

            builder
                .Register(c => new SqlCompactConnectionFactory(this.GetConnectionStringToDb()))
                .As<SqlCompactConnectionFactory>()
                .SingleInstance();
            builder
                .RegisterType<SqlCompactUserRepository>()
                .As<IUserRepository>()
                .InstancePerRequest();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            IContainer container = builder.Build();

            HttpConfiguration config = GlobalConfiguration.Configuration;
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private string GetConnectionStringToDb()
        {
            string pathToSqlCeDatabase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Databases\idsrv_accounts.sdf");
            return $"Data Source={pathToSqlCeDatabase}";
        }
    }
}
