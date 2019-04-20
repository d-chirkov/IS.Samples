using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IS.Controllers
{
    public class ApplicationsController : Controller
    {
        public ActionResult GetAll()
        {
            return View();
        }
    }
}