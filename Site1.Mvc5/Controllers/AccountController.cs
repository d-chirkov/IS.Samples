using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdSrv.AspNet.Helpers;
using Site1.Mvc5.Attributes;
using Site1.Mvc5.Models;

namespace Site1.Mvc5.Controllers
{
    public class AccountController : Controller
    {
        [LocalAuthorize(false)]
        public ActionResult SignIn(string returnUrl)
        {
            return this.Redirect((returnUrl != null && this.Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        [LocalAuthorize(false)]
        public void SignOut()
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }

        [LocalAuthorize(false)]
        public async Task<ActionResult> AccessDenied()
        {
            string userName = await IdSrvConnection.GetUserNameAsync(this.HttpContext);
            return this.View((object)userName);
        }

        [HttpGet]
        [LocalAuthorize(true)]
        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        [LocalAuthorize(true)]
        public async Task<ActionResult> Register(RegisterForm registerForm)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(registerForm);
            }
            NewUser newUser = null;
            using (var client = new HttpClient())
            {
                // Update port # in the following line.
                client.BaseAddress = new Uri("https://localhost:44301");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsJsonAsync(
                   "api/register/user", new { Name = registerForm.Login, Password = registerForm.Password });
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Conflict)
                {
                    this.ModelState.AddModelError("Couldn't create", "Не удалось зарегистрировать нового пользователя на SSO сервере");
                    return this.View(registerForm);
                }
                this.ViewBag.AlreadyExists = response.StatusCode == HttpStatusCode.Conflict;
                newUser = await response.Content.ReadAsAsync<NewUser>();
            }
            using (var context = new AccountsContext())
            {
                context.UserProfiles.Add(new UserProfile { IdSrvId = newUser.Id, Login = newUser.Name });
                // Тут моет рухнуть с исключением, никак пока не обрабатываю
                await context.SaveChangesAsync();
            }
            this.ViewBag.Name = newUser.Name;
            return this.View("UserCreated");
        }

        private class NewUser
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}