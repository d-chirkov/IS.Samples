using IdentityServer3.Core;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Site2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string userName = (Request.GetOwinContext().Authentication.User as ClaimsPrincipal)?.FindFirst(Constants.ClaimTypes.Name)?.Value;
            return View(userName as object);
        }

        // Чтобы потребовать входа пользователя, достаточно добавить Authorize атрибут
        [Authorize]
        public ActionResult Account()
        {
            var user = Request.GetOwinContext().Authentication.User;
            string userId = (user as ClaimsPrincipal).FindFirst(Constants.ClaimTypes.Subject).Value;
            string userName = (user as ClaimsPrincipal).FindFirst(Constants.ClaimTypes.Name).Value;
            return View(userName as object);
        }

        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }
    }
}