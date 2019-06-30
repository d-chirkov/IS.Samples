namespace Site1.Mvc5.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Site1.Mvc5.Attributes;
    using Site1.Mvc5.Models;
    using IdSrv.AspNet.Helpers;
    using System.Threading.Tasks;

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