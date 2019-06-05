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

        // GET: Users
        public ViewResult Index()
        {
            IEnumerable<IdSrvUser> users = this.AccountService.GetUsers();
            return View(users);
        }
    }
}