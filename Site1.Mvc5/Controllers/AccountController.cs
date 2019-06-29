namespace Site1.Mvc5.Controllers
{
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
    using IdSrv.Account.Models;

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
            NewUser createdUser = null;
            var userDto = new NewIdSrvUserDTO { UserName = registerForm.Login, Password = registerForm.Password };
            using (var client = new HttpClient())
            {
                // Update port # in the following line.
                client.BaseAddress = new Uri("https://localhost:44397/Api/User/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PutAsJsonAsync("", userDto);
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Conflict)
                {
                    this.ModelState.AddModelError("Couldn't create", "Не удалось зарегистрировать нового пользователя на SSO сервере");
                    return this.View(registerForm);
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    this.ViewBag.AlreadyExists = true;
                }

                response = await client.GetAsync($"GetByUserName?userName={HttpUtility.UrlEncode(userDto.UserName)}");
                if (!response.IsSuccessStatusCode)
                {
                    this.ModelState.AddModelError("Couldn't create", "Ошибка при чтении созданного пользователя на SSO сервере");
                    return this.View(registerForm);
                }
                var createdUserDto = await response.Content.ReadAsAsync<IdSrvUserDTO>();
                createdUser = new NewUser { Id = createdUserDto.Id.ToString(), Name = createdUserDto.UserName };
            }
            using (var context = new AccountsContext())
            {
                context.UserProfiles.Add(new UserProfile { IdSrvId = createdUser.Id, Login = createdUser.Name });
                // Тут моет рухнуть с исключением, никак пока не обрабатываю
                await context.SaveChangesAsync();
            }
            this.ViewBag.Name = createdUser.Name;
            return this.View("UserCreated");
        }

        private class NewUser
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}