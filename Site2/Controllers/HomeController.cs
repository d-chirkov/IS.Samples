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
            if (!UserAccessControl.HasAccess(userId))
            {
                return RedirectToAction("AccessDenied");
            }

            string userName = (user as ClaimsPrincipal).FindFirst(Constants.ClaimTypes.Name).Value;
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
            string userName = user.FindFirst(Constants.ClaimTypes.Name).Value;
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