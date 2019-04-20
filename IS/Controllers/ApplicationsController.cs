using IS.Repos;
using IS.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace IS.Controllers
{
    public class ApplicationsController : Controller
    {
        public ActionResult GetAll()
        {
            var allApplcations = new AllApplicationsViewModel
            {
                Applications = ClientRepo.GetAllClients().Select(u => new ApplicationViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Uri = u.Uri == string.Empty ? "-" : u.Uri
                })
            };
            return View(allApplcations);
        }
    }
}