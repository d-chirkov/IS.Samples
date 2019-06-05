using IdSrv.Account.WebControl.Infrastructure.Abstractions;
using IdSrv.Account.WebControl.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdSrv.Account.WebControl.Controllers
{
    public class UsersController : Controller
    {
        private IAccountService AccountService { get; set; }

        public UsersController(IAccountService accountService)
        {
            this.AccountService = accountService;
        }

        [HttpGet]
        public ViewResult Index()
        {
            IEnumerable<IdSrvUser> users = this.AccountService.GetUsers();
            return View(users);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Create(NewIdSrvUser newUser)
        {
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }
            bool created = this.AccountService.CreateUser(newUser);
            if (!created)
            {
                ModelState.AddModelError("", "Такой пользователь уже существует");
            }
            return created ? View(nameof(Index)) : View(newUser);
        }
    }
}