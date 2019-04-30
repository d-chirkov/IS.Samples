using Site1.Mvc5.Attributes;
using Site1.Mvc5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Site1.Mvc5.Controllers
{
    public class AccountController : Controller
    {
        [LocalAuthorize(false)]
        public ActionResult SignIn(string returnUrl)
        {
            return Redirect((returnUrl != null && Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        [LocalAuthorize(false)]
        public void SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
        }

        [LocalAuthorize(false)]
        public ActionResult AccessDenied()
        {
            string userName = (Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                ?.FindFirst(OidcClaimTypes.Name)
                ?.Value;
            return View((object)userName);
        }

        [HttpGet]
        [LocalAuthorize(true)]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [LocalAuthorize(true)]
        public async Task<ActionResult> Register(RegisterForm registerForm)
        {
            if (!ModelState.IsValid)
            {
                return View(registerForm);
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
                   "api/register/user", new { Name = registerForm.Login, Password = registerForm.Password});
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("Couldn't create", "Не удалось зарегистрировать нового пользователя на SSO сервере");
                    return View(registerForm);
                }
                ViewBag.AlreadyExists = response.StatusCode == HttpStatusCode.Conflict;
                newUser = await response.Content.ReadAsAsync<NewUser>();
            }
            using (var context = new AccountsContext())
            {
                context.UserProfiles.Add(new UserProfile { IdSrvId = newUser.Id, Login = newUser.Name });
                // Тут моет рухнуть с исключением, никак пока не обрабатываю
                await context.SaveChangesAsync();
            }
            ViewBag.Name = newUser.Name;
            return View("UserCreated");
        }

        class NewUser
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}