namespace Site2.Mvc5.Controllers
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Контроллер аккаунтов, содержит методы для входа и выхода пользователей.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Метод для входа пользователей.
        /// </summary>
        /// <param name="returnUrl">Обратный url, на который надо переадресовать после успешного входа.</param>
        /// <returns>Редиректит на <paramref name="returnUrl"/>, если он не null, иначе на ~/ .</returns>
        [Authorize]
        public ActionResult SignIn(string returnUrl)
        {
            return this.Redirect((returnUrl != null && this.Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        /// <summary>
        /// Метод выхода, вызывается соответствующий метод Owin-контекста, что автоматически вызовет выход
        /// на identity server (то есть на всех сайтах сразу).
        /// </summary>
        [Authorize]
        public void SignOut()
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }
    }
}