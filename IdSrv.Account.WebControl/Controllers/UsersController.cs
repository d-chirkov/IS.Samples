﻿namespace IdSrv.Account.WebControl.Controllers
{
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using IdSrv.Account.WebControl.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    public class UsersController : Controller
    {
        private IAccountService AccountService { get; set; }

        public UsersController(IAccountService accountService)
        {
            this.AccountService = accountService;
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
            return created ? View(nameof(Index)) : View(newUser);
        }

        [HttpGet]
        public ViewResult ChangePassword(Guid id)
        {
            return View(new ChangeIdSrvUserPasswordDTO { UserId = id });
        }

        [HttpGet]
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
                ModelState.AddModelError("", "Старыль пароль указан неверно");
            }
            return changed ? View(nameof(Index)) : View(new ChangeIdSrvUserPasswordDTO { UserId = passwords.UserId });
        }
    }
}