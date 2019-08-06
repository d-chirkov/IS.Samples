namespace IdSrv.Account.WebControl.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Контроллер для взаимодействия с пользователями identity sevrver через web-интерфейс.
    /// Позволяет выплонять CRUD операции с пользователями.
    /// </summary>
    [Authorize]
    public class UsersController : Controller
    {
        /// <summary>
        /// Инициализирует контроллер.
        /// </summary>
        /// <param name="userService">Сервис для доступа к месту хранения пользователей (rest-сервис).</param>
        public UsersController(IUserService userService)
        {
            this.UserService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Получает или задает сервис для доступа к месту хранения пользователей (rest-сервис).
        /// </summary>
        private IUserService UserService { get; set; }

        /// <summary>
        /// Отобразить страницу со всеми сущетсвующими пользователями.
        /// </summary>
        /// <returns>View с пользователями для отображения.</returns>
        [HttpGet]
        public async Task<ViewResult> Index()
        {
            IEnumerable<IdSrvUserDto> users = await this.UserService.GetUsersAsync();
            return this.View(users);
        }

        /// <summary>
        /// Отобразить страницу создания нового пользователя.
        /// </summary>
        /// <returns>View для отображения.</returns>
        [HttpGet]
        public ViewResult Create()
        {
            return this.View();
        }

        /// <summary>
        /// Создать нового пользователя через сервис.
        /// </summary>
        /// <param name="user">Данные для создания нового пользователя.</param>
        /// <returns>
        /// Переадресует на Index с сообщением об успехе или, в случае ошибки во входных данных,
        /// страницу с формой создания пользователя с подсвеченными неверными полями.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> Create(NewIdSrvUserDto user)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(user);
            }

            bool created = await this.UserService.CreateUserAsync(user);
            if (!created)
            {
                this.ModelState.AddModelError(string.Empty, "Такой пользователь уже существует");
            }

            return created ? this.ViewSuccess("Пользователь успешно создан") : this.View(user) as ActionResult;
        }

        /// <summary>
        /// Отобразить страницу обновления пароля пользователя.
        /// </summary>
        /// <param name="id">id пользователя, для которого надо обновить пароль.</param>
        /// <returns>
        /// View с id пользователя, для которого необходимо обновить пароль.
        /// </returns>
        [HttpGet]
        public ViewResult ChangePassword(Guid id)
        {
            return this.View(new IdSrvUserPasswordDto { Id = id });
        }

        /// <summary>
        /// Обновить пароль пользователя.
        /// </summary>
        /// <param name="passwords">Данные для обновления: id пользователя и новый пароль для него.</param>
        /// <returns>
        /// В случае успешного обнолвения, переадресует на Index с сообщением об успехе,
        /// в противном случае снова отобразит форму обновления пароля с подсвеченными неправильными полями.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> ChangePassword(IdSrvUserPasswordDto passwords)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(new IdSrvUserPasswordDto { Id = passwords.Id });
            }

            bool changed = await this.UserService.ChangePasswordForUserAsync(passwords);
            if (!changed)
            {
                this.ModelState.AddModelError(string.Empty, "Не удалось изменить пароль");
            }

            return changed ?
                    this.ViewSuccess("Пароль успешно изменён") :
                    this.View(new IdSrvUserPasswordDto { Id = passwords.Id }) as ActionResult;
        }

        /// <summary>
        /// Удалить пользователя по его id.
        /// </summary>
        /// <param name="id">id пользователя.</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Users/Delete")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await this.UserService.DeleteUserAsync(id);
            return deleted ? this.ViewSuccess("Пользователь успешно удалён") : this.ViewError("Не удалось удалить пользователя");
        }

        /// <summary>
        /// Заблокировать пользователя по его id.
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Users/Block")]
        public async Task<ActionResult> Block(Guid id)
        {
            bool blocked = await this.UserService.ChangeBlock(new IdSrvUserBlockDto { Id = id, IsBlocked = true });
            return blocked ?
                this.ViewSuccess("Пользователь заблокирован") :
                this.ViewError("Не удалось заблокировать пользователя");
        }

        /// <summary>
        /// Разаблокировать пользователя по его id.
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Users/Unblock")]
        public async Task<ActionResult> Unblock(Guid id)
        {
            bool unblocked = await this.UserService.ChangeBlock(new IdSrvUserBlockDto { Id = id, IsBlocked = false });
            return unblocked ?
                this.ViewSuccess("Пользователь разблокирован") :
                this.ViewError("Не удалось разблокировать пользователя");
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об успехе.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением об успехе в TempData.
        /// </returns>
        private RedirectToRouteResult ViewSuccess(string message)
        {
            return this.ViewMessage(message, false);
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением об ошибке в TempData.
        /// </returns>
        private RedirectToRouteResult ViewError(string message)
        {
            return this.ViewMessage(message, true);
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об успехе или ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="isError">true, если это сообщение об ошибке.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением в секции _Message структуры TempData
        /// и его значением _IsError равным true или false (сообщение об ошибке или успехе соответственно).
        /// </returns>
        private RedirectToRouteResult ViewMessage(string message, bool isError)
        {
            this.TempData["_IsError"] = isError;
            this.TempData["_Message"] = message;
            return this.RedirectToAction(nameof(this.Index));
        }
    }
}