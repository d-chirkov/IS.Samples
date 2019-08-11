namespace Site3.WebForms
{
    using System;
    using System.Web;
    using System.Web.Routing;

    /// <summary>
    /// Главный класс приложения, содержит точку входа сервиса.
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>
        /// Событие старта сайта на сервере IIS.
        /// </summary>
        /// <param name="sender">Отправиль события.</param>
        /// <param name="e">Аргументы события.</param>
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}