namespace IdSrv.Account.WebControl.Controllers
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Главный контроллер, содержит методы, общие для всех страниц (кнопок на верхней панели).
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Домашняя страница сайта.
        /// </summary>
        /// <returns>
        /// View домашней страницы.
        /// </returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Страница About.
        /// </summary>
        /// <returns>
        /// View страницы About.
        /// </returns>
        public ActionResult About()
        {
            this.ViewBag.Message = "Your application description page.";

            return this.View();
        }

        /// <summary>
        /// Страница Contact.
        /// </summary>
        /// <returns>
        /// View страницы Contact.
        /// </returns>
        public ActionResult Contact()
        {
            this.ViewBag.Message = "Your contact page.";

            return this.View();
        }

        /// <summary>
        /// Адрес для входа пользователей-администраторов через identity server.
        /// Вход вполняется благодаря применению аттрибута <see cref="AuthorizeAttribute"/>.
        /// </summary>
        /// <param name="returnUrl">Адрес, на который надо вернутся после входа.</param>
        /// <returns>Релиректит на адрес, переданный как параметр <paramref name="returnUrl"/>.</returns>
        [Authorize]
        public ActionResult SignIn(string returnUrl)
        {
            return this.Redirect((returnUrl != null && this.Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        /// <summary>
        /// Выход пользователя из identity server (выход происходит сразу со всех сайтов).
        /// </summary>
        [Authorize]
        public void SignOut()
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }
    }
}