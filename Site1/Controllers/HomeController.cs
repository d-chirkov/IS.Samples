using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Site1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string userName = (Request.GetOwinContext().Authentication.User as ClaimsPrincipal)?.FindFirst("name")?.Value;
            return View(userName as object);
        }

        // Чтобы потребовать входа пользователя, достаточно добавить Authorize атрибут
        [Authorize]
        public ActionResult Account()
        { 
            // Так можно получить пользователя, который делает запрос, а из пользователя
            // можно получить его Claim-ы, там будет всё, что мы указали в IS.Users
            var user = Request.GetOwinContext().Authentication.User;

            // Получение идентификатора пользователя. Мы его не указывали как отдельный Claim, потому что
            // он автоматически копируется в Claim-ы пользователя
            string userId = (user as ClaimsPrincipal).FindFirst("sub").Value;

            // Проверка, что пользователь имеет доступ к сайту. Если нет, то редиректим на страницу AccessDenied
            // Имеет смысл обернуть AuthorizeAttribute, чтобы проверка была там.
            if (!UserAccessControl.HasAccess(userId))
            {
                return RedirectToAction("AccessDenied");
            }

            // Получение логина пользователя, его мы отдельно указывали в Claim-ах пользователя 
            // по ключу given_name
            // Но при использовании секрета клиента надо дополнительно запрашивать его у IdentityServer, скоро сделаю
            string userName = (user as ClaimsPrincipal).FindFirst("name").Value;

            return View(userName as object);
        }
        
        public ActionResult AccessDenied()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            string redirectToUrl = Url.Action("Index");
            if (Request.UrlReferrer != null && Request.Url.Host == Request.UrlReferrer.Host)
            {
                redirectToUrl = Request.UrlReferrer.ToString();
            }
            Response.AddHeader("REFRESH", $"2;{redirectToUrl}");
            ClaimsPrincipal user = Request.GetOwinContext().Authentication.User;
            string userName = user.FindFirst("name").Value;
            return View(userName as object);
        }

        public ActionResult Logout()
        {
            if (Request.GetOwinContext().Authentication.User.Identity.IsAuthenticated)
            {
                Request.GetOwinContext().Authentication.SignOut();
            }
            return Redirect("/");
        }

    }
}