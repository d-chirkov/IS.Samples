namespace Site2.Mvc5.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Имитирует сайт, использующий учётные записи identity server.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Вывод домащнюю страницу.
        /// </summary>
        /// <returns>Домашняя страница сайта.</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Вывод страницу About.
        /// </summary>
        /// <returns>Страницу About из шаблона.</returns>
        [Authorize]
        public ActionResult About()
        {
            this.ViewBag.Message = "Your application description page.";

            return this.View();
        }
    }
}