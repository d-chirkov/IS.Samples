namespace Site1.Mvc5.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.AspNet.Helpers;
    using Site1.Mvc5.Attributes;
    using Site1.Mvc5.Models;

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
        public ActionResult About()
        {
            this.ViewBag.Message = "Your application description page.";

            return this.View();
        }

        /// <summary>
        /// Имитирует страницу, доступ к которой есть только зарегестриорванных на данном сайта пользователей
        /// (то есть пользователей, которые есть в локальной БД). Поэтому в <see cref="LocalAuthorizeAttribute"/>
        /// передаётся аргумент true - проверить пользователя в локальной базе данных.
        /// </summary>
        /// <returns>
        /// Страницу с идентификатором пользователя в локальной БД, либо страницу
        /// ~/Account/AccessDenied - у пользователя нет доступа к этой страницу (если его нет в локальной БД).
        /// </returns>
        [LocalAuthorize(true)]
        public async Task<ActionResult> UserProfile()
        {
            string idsrvUserId = await IdSrvConnection.GetUserIdAsync(this.HttpContext);
            string userLogin = await IdSrvConnection.GetUserNameAsync(this.HttpContext);

            UserProfile userProfile = null;
            using (var context = new AccountsContext())
            {
                userProfile =
                    context.UserProfiles.Where(p => p.IdSrvId == idsrvUserId).FirstOrDefault() ??
                    context.UserProfiles.Where(p => p.Login == userLogin).FirstOrDefault();
            }

            if (userProfile == null)
            {
                return this.HttpNotFound();
            }

            return this.View(userProfile);
        }
    }
}