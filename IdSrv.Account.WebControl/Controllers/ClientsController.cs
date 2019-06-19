namespace IdSrv.Account.WebControl.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    public class ClientsController : Controller
    {
        private IClientService ClientService { get; set; }

        public ClientsController(IClientService clientService)
        {
            this.ClientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            IEnumerable<IdSrvClientDTO> clients = await this.ClientService.GetClientsAsync();
            return this.View(clients);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(NewIdSrvClientDTO client)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(client);
            }
            bool created = await this.ClientService.CreateClientAsync(client);
            if (!created)
            {
                this.ModelState.AddModelError("", "Такой клиент уже существует");
            }
            return created ? this.ViewSuccess("Клиент успешно создан") : this.View(client) as ActionResult;
        }

        [HttpGet]
        public async Task<ActionResult> Update(Guid id)
        {
            IdSrvClientDTO client = await this.ClientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return this.ViewError("Такого клиента не существует") as ActionResult; ;
            }
            var updateClient = new UpdateIdSrvClientDTO
            {
                Id = client.Id,
                Name = client.Name,
                Secret = client.Secret,
                Uri = client.Uri
            };
            return this.View(updateClient);
        }

        [HttpPost]
        public async Task<ActionResult> Update(UpdateIdSrvClientDTO client)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(client);
            }
            bool updated = await this.ClientService.UpdateClientAsync(client);
            if (!updated)
            {
                this.ModelState.AddModelError("", "Не удалось обновить клиента");
            }
            return updated ? this.ViewSuccess("Клиент успешно обновлён") : this.View(client) as ActionResult;
        }

        [HttpPost]
        [Route("/Clients/Delete")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await this.ClientService.DeleteClientAsync(id);
            return deleted ? this.ViewSuccess("Клиент успешно удалён") : this.ViewError("Не удалось удалить клиента");
        }

        [HttpPost]
        [Route("/Clients/Block")]
        public async Task<ActionResult> Block(Guid id)
        {
            bool blocked = await this.ClientService.ChangeBlock(new IdSrvClientBlockDTO { Id = id, IsBlocked = true });
            return blocked ? 
                this.ViewSuccess("Клиент заблокирован") : 
                this.ViewError("Не удалось заблокировать клиента");
        }

        [HttpPost]
        [Route("/Clients/Unblock")]
        public async Task<ActionResult> Unblock(Guid id)
        {
            bool unblocked = await this.ClientService.ChangeBlock(new IdSrvClientBlockDTO { Id = id, IsBlocked = false });
            return unblocked ?
                this.ViewSuccess("Клиент разблокирован") :
                this.ViewError("Не удалось разблокировать клиента");
        }

        private RedirectToRouteResult ViewSuccess(string message)
        {
            return this.ViewMessage(message, false);
        }

        private RedirectToRouteResult ViewError(string message)
        {
            return this.ViewMessage(message, true);
        }

        private RedirectToRouteResult ViewMessage(string message, bool isError)
        {
            this.TempData["_IsError"] = isError;
            this.TempData["_Message"] = message;
            return this.RedirectToAction(nameof(Index));
        }
    }
}