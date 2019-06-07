using IdSrv.Account.WebControl.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdSrv.Account.WebControl.Controllers
{
    public class ApplicationsController : Controller
    {
        public IApplicationService ApplicationService { get; set; }

        public ApplicationsController(IApplicationService applicationService)
        {
            this.ApplicationService = applicationService ?? throw new NullReferenceException(nameof(applicationService));
        }
        // GET: Applications
        public ActionResult Index()
        {
            return View();
        }
    }
}