using IdSrv.Account.WebControl.Infrastructure.Abstractions;
using IdSrv.Account.WebControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ViewResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ViewResult> Create()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ViewResult> Create(NewIdSrvApplicationDTO application)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<ViewResult> Update(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ViewResult> Update(IdSrvApplicationDTO application)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<ViewResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}