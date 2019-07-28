namespace IdSrv.Account.WebControl.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    [Authorize]
    public class UsersController : Controller
    {
        private IUserService userService { get; set; }

        public UsersController(IUserService userService)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            IEnumerable<IdSrvUserDto> users = await this.userService.GetUsersAsync();
            return this.View(users);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(NewIdSrvUserDto user)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(user);
            }
            bool created = await this.userService.CreateUserAsync(user);
            if (!created)
            {
                this.ModelState.AddModelError("", "Такой пользователь уже существует");
            }
            return created ? this.ViewSuccess("Пользователь успешно создан") : this.View(user) as ActionResult;
        }

        [HttpGet]
        public ViewResult ChangePassword(Guid id)
        {
            return this.View(new IdSrvUserPasswordDto { Id = id });
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(IdSrvUserPasswordDto passwords)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(new IdSrvUserPasswordDto { Id = passwords.Id });
            }
            bool changed = await this.userService.ChangePasswordForUserAsync(passwords);
            if (!changed)
            {
                this.ModelState.AddModelError("", "Не удалось изменить пароль");
            }
            return changed ? 
                    this.ViewSuccess("Пароль успешно изменён") :
                    this.View(new IdSrvUserPasswordDto { Id = passwords.Id }) as ActionResult;
        }

        [HttpPost]
        [Route("/Users/Delete")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await this.userService.DeleteUserAsync(id);
            return deleted ? this.ViewSuccess("Пользователь успешно удалён") : this.ViewError("Не удалось удалить пользователя");
        }

        [HttpPost]
        [Route("/Users/Block")]
        public async Task<ActionResult> Block(Guid id)
        {
            bool blocked = await this.userService.ChangeBlock(new IdSrvUserBlockDto { Id = id, IsBlocked = true });
            return blocked ? 
                this.ViewSuccess("Пользователь заблокирован") : 
                this.ViewError("Не удалось заблокировать пользователя");
        }

        [HttpPost]
        [Route("/Users/Unblock")]
        public async Task<ActionResult> Unblock(Guid id)
        {
            bool unblocked = await this.userService.ChangeBlock(new IdSrvUserBlockDto { Id = id, IsBlocked = false });
            return unblocked ?
                this.ViewSuccess("Пользователь разблокирован") :
                this.ViewError("Не удалось разблокировать пользователя");
        }

        private RedirectToRouteResult ViewSuccess(string message)
        {
            return this.ViewMessage(message, false);
        }

        private RedirectToRouteResult ViewError(string message)
        {
            return this.ViewMessage(message, true);
        }

        private RedirectToRouteResult ViewMessage(string message, bool isError)
        {
            this.TempData["_IsError"] = isError;
            this.TempData["_Message"] = message;
            return this.RedirectToAction(nameof(Index));
        }
    }
}