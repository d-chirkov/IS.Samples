namespace IS.Controllers
{
    using IS.ViewModels;
    using SharedLib.IS;
    using System.Linq;
    using System.Web.Mvc;

    public class ClientsController : Controller
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

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(NewClientViewModel newClient)
        {
            if (!ModelState.IsValid)
            {
                return View(newClient);
            }
            bool created = ClientRepo.SetClient(newClient.Id, newClient.Name, newClient.Secret, newClient.Uri);
            if (!created)
            {
                ModelState.AddModelError("Couldn't create", "Couldn't create new client");
            }
            return created ? (ActionResult)RedirectToAction(nameof(GetAll)) : View(newClient);
        }

        public ActionResult Edit(string id)
        {
            ISClient client = ClientRepo.GetClient(id);
            if (client == null)
            {
                return new HttpNotFoundResult("Клиент не найден");
            }
            var viewModel = new EditClientViewModel
            {
                Id = client.Id,
                Name = client.Name,
                Uri = client.Uri
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditClientViewModel editClient)
        {
            if (!ModelState.IsValid)
            {
                return View(editClient);
            }
            bool edited = ClientRepo.UpdateClient(editClient.Id, editClient.Name, editClient.Secret, editClient.Uri);
            if (!edited)
            {
                ModelState.AddModelError("Couldn't edit", "Couldn't edit this client");
            }
            return edited ? (ActionResult)RedirectToAction(nameof(GetAll)) : View(editClient);
        }

        public ActionResult Delete(string id)
        {
            bool deleted = ClientRepo.DeleteClient(id);
            return deleted ? (ActionResult)RedirectToAction(nameof(GetAll)) : new HttpNotFoundResult("Клиент не найден");
        }
    }
}