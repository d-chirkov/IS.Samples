namespace Site1.Mvc5.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.AspNet.Helpers;
    using Site1.Mvc5.Attributes;
    using Site1.Mvc5.Models;

    /// <summary>
    /// Контроллер аккаунтов, содержит методы для входа, выхода, регистрации пользователей, а также
    /// страницу для вывода сообщения об ошибке доступа.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Метод для входа пользователей.
        /// </summary>
        /// <param name="returnUrl">Обратный url, на который надо переадресовать после успешного входа.</param>
        /// <returns>Редиректит на <paramref name="returnUrl"/>, если он не null, иначе на ~/ .</returns>
        [LocalAuthorize(false)]
        public ActionResult SignIn(string returnUrl)
        {
            return this.Redirect((returnUrl != null && this.Url.IsLocalUrl(returnUrl)) ? returnUrl : "~/");
        }

        /// <summary>
        /// Метод выхода, вызывается соответствующий метод Owin-контекста, что автоматически вызовет выход
        /// на identity server (то есть на всех сайтах сразу).
        /// </summary>
        [LocalAuthorize(false)]
        public void SignOut()
        {
            this.Request.GetOwinContext().Authentication.SignOut();
        }

        /// <summary>
        /// Выводит страницу отсутствия доступа. Вывод такой страницы необходим тогда, когда пользователь
        /// зашёл на identity server, но отсутствует в БД данного сайта, и ему не разрешён вход на определённые
        /// страницы.
        /// </summary>
        /// <returns>
        /// Страницу с информацией о том, что пользователю не разрешён вход, страницу содержит логин пользователя.
        /// </returns>
        [LocalAuthorize(false)]
        public async Task<ActionResult> AccessDenied()
        {
            string userName = await IdSrvConnection.GetUserNameAsync(this.HttpContext);
            return this.View((object)userName);
        }

        /// <summary>
        /// Вывод страницу регистрации пользователя. Выводится страницу именно этого сайта (identity server тут
        /// пока не причём, смотри Post-метод).
        /// </summary>
        /// <returns>Страницу с формой регистрации пользователя.</returns>
        [HttpGet]
        [LocalAuthorize(true)]
        public ActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// Регистрирует пользователя (если с данными всё в порядке). При этом идёт обращение к WebApi с базой
        /// пользователей (и клиентов) - создаётся новый пользователь там и в локальной БД. Если пользователь
        /// уже есть в базе WebApi, то он просто добавляется в локальную БД.
        /// </summary>
        /// <param name="registerForm">Данные формы регистрации.</param>
        /// <returns>Вывод страницы с информации либо об успехе, либо об ошибке.</returns>
        [HttpPost]
        [LocalAuthorize(true)]
        public async Task<ActionResult> Register(RegisterForm registerForm)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(registerForm);
            }

            this.ViewBag.AlreadyExists = false;
            NewUser createdUser = null;
            var userDto = new NewIdSrvUserDto { UserName = registerForm.Login, Password = registerForm.Password };
            using (var client = new HttpClient())
            {
                // Update port # in the following line.
                client.BaseAddress = new Uri("https://localhost:44397/Api/User/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PutAsJsonAsync(string.Empty, userDto);
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

                var createdUserDto = await response.Content.ReadAsAsync<IdSrvUserDto>();
                createdUser = new NewUser { Id = createdUserDto.Id.ToString(), Name = createdUserDto.UserName };
            }

            using (var context = new AccountsContext())
            {
                context.UserProfiles.Add(new UserProfile { IdSrvId = createdUser.Id, Login = createdUser.Name });

                // Тут может рухнуть с исключением, никак пока не обрабатываю
                await context.SaveChangesAsync();
            }

            this.ViewBag.Name = createdUser.Name;
            return this.View("UserCreated");
        }

        /// <summary>
        /// Класс для передачи данных во View.
        /// </summary>
        private class NewUser
        {
            /// <summary>
            /// Получает или задает идентификатор пользователя (Guid).
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Получает или задает логин пользователя.
            /// </summary>
            public string Name { get; set; }
        }
    }
}