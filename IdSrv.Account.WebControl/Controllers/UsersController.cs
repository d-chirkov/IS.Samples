namespace IdSrv.Account.WebControl.Controllers
{
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;
    using IdSrv.Account.WebControl.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        public ViewResult Index()
        {
            IEnumerable<IdSrvUserDTO> users = this.AccountService.GetUsers();
            return View(users);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Create(NewIdSrvUserDTO newUser)
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

        [HttpGet]
        public ViewResult ChangePassword(Guid id)
        {
            return View(new ChangeIdSrvUserPasswordDTO { UserId = id });
        }

        [HttpGet]
        public ViewResult ChangePassword(ChangeIdSrvUserPasswordDTO passwords)
        {
            throw new NotImplementedException();
        }
    }
}