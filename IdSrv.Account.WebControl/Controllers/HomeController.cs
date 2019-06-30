namespace IdSrv.Account.WebControl.Controllers
{
    using System.Web;
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult About()
        {
            this.ViewBag.Message = "Your application description page.";

            return this.View();
        }

        public ActionResult Contact()
        {
            this.ViewBag.Message = "Your contact page.";

            return this.View();
        }

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