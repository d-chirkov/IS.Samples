namespace Site2.Mvc5.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using IdSrv.AspNet.Helpers;

    public class AccountController : Controller
    {
        [Authorize]
        public ActionResult SignIn(string returnUrl)
        {
            return this.Redirect((returnUrl != null && this.Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        [Authorize]
        public void SignOut()
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }
    }
}