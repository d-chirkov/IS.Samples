namespace IdSrv.Account.WebControl.Controllers
{
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using IdSrv.Account.WebControl.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class UsersController : Controller
    {
        private IUserService AccountService { get; set; }

        public UsersController(IUserService accountService)
        {
            this.AccountService = accountService ?? throw new NullReferenceException(nameof(accountService));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            IEnumerable<IdSrvUserDTO> users = await this.AccountService.GetUsersAsync();
            return View(users);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ViewResult> Create(NewIdSrvUserDTO newUser)
        {
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }
            bool created = await this.AccountService.CreateUserAsync(newUser);
            if (!created)
            {
                ModelState.AddModelError("", "Такой пользователь уже существует");
            }
            return created ? this.ViewSuccess("Пользователь успешно создан") : View(newUser);
        }

        [HttpGet]
        public ViewResult ChangePassword(Guid id)
        {
            return View(new ChangeIdSrvUserPasswordDTO { UserId = id });
        }

        [HttpPost]
        public async Task<ViewResult> ChangePassword(ChangeIdSrvUserPasswordDTO passwords)
        {
            if (passwords.NewPassword != passwords.RepeatNewPassword)
            {
                ModelState.AddModelError("", "Пароли не совпадают");
            }
            if (!ModelState.IsValid)
            {
                return View(new ChangeIdSrvUserPasswordDTO { UserId = passwords.UserId });
            }
            bool changed = await this.AccountService.ChangePasswordForUserAsync(passwords);
            if (!changed)
            {
                ModelState.AddModelError("", "Старый пароль указан неверно");
            }
            return changed ? this.ViewSuccess("Пароль успешно изменён") : View(new ChangeIdSrvUserPasswordDTO { UserId = passwords.UserId });
        }

        [HttpPost]
        public async Task<ViewResult> Delete(Guid id)
        {
            bool deleted = await this.AccountService.DeleteUserAsync(id);
            return deleted ? this.ViewSuccess("Пользователь успешно удалён") : this.ViewError("Не удалось удалить пользователя");
        }

        private ViewResult ViewSuccess(string message) => ViewMessage(message, false);

        private ViewResult ViewError(string message) => ViewMessage(message, true);

        private ViewResult ViewMessage(string message, bool isError)
        {
            TempData["_IsError"] = isError;
            TempData["_Message"] = message;
            return View(nameof(Index));
        }
    }
}