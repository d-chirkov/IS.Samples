﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Site1.Mvc5.Attributes;
using Site1.Mvc5.Models;

namespace Site1.Mvc5.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [LocalAuthorize(true)]
        public ActionResult UserProfile()
        {
            int idsrvUserId = int.Parse((Request.GetOwinContext().Authentication.User as System.Security.Claims.ClaimsPrincipal)
                ?.FindFirst(OidcClaimTypes.Subject)
                ?.Value);
            UserProfile userProfile = null;
            using (var context = new AccountsContext())
            {
                userProfile = context.UserProfiles.Where(p => p.IdSrvId == idsrvUserId).FirstOrDefault();
            }
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }
    }
}