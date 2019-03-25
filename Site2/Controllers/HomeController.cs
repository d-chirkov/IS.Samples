﻿using IdentityServer3.Core;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Site2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string userName = (Request.GetOwinContext().Authentication.User as ClaimsPrincipal)?.FindFirst(Constants.ClaimTypes.Name)?.Value;
            return View(userName as object);
        }

        // Чтобы потребовать входа пользователя, достаточно добавить Authorize атрибут
        [Authorize]
        public ActionResult Account()
        {
            // Так можно получить пользователя, который делает запрос, а из пользователя
            // можно получить его Claim-ы, там будет всё, что мы указали в IS.Users
            var user = Request.GetOwinContext().Authentication.User;

            // Получение идентификатора пользователя. Мы его не указывали как отдельный Claim, потому что
            // он автоматически копируется в Claim-ы пользователя
            string userId = (user as ClaimsPrincipal).FindFirst(Constants.ClaimTypes.Subject).Value;

            // Получение логина пользователя, его мы отдельно указывали в Claim-ах пользователя 
            // по ключу given_name
            string userName = (user as ClaimsPrincipal).FindFirst(Constants.ClaimTypes.Name).Value;
            return View(userName as object);
        }

        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
        }
    }
}