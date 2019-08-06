namespace IdSrv.Account.WebControl.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebControl.Infrastructure.Abstractions;

    /// <summary>
    /// Контроллер для взаимодействия с клиентами identity sevrver через web-интерфейс.
    /// Позволяет выплонять CRUD операции с клиентами.
    /// </summary>
    [Authorize]
    public class ClientsController : Controller
    {
        /// <summary>
        /// Инициализирует контроллер.
        /// </summary>
        /// <param name="clientService">Сервис для доступа к месту хранения клиентов (rest-сервис).</param>
        public ClientsController(IClientService clientService)
        {
            this.ClientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        /// <summary>
        /// Получает или задает сервис для доступа к месту хранения клиентов (rest-сервис).
        /// </summary>
        private IClientService ClientService { get; set; }

        /// <summary>
        /// Отобразить страницу со всеми сущетсвующими клиентами.
        /// </summary>
        /// <returns>View с клиентами для отображения.</returns>
        [HttpGet]
        public async Task<ViewResult> Index()
        {
            IEnumerable<IdSrvClientDto> clients = await this.ClientService.GetClientsAsync();
            return this.View(clients);
        }

        /// <summary>
        /// Отобразить страницу создания нового клиента.
        /// </summary>
        /// <returns>View для отображения.</returns>
        [HttpGet]
        public ViewResult Create()
        {
            return this.View();
        }

        /// <summary>
        /// Создать нового клиента через сервис.
        /// </summary>
        /// <param name="client">Данные для создания нового клиента.</param>
        /// <returns>
        /// Переадресует на Index с сообщением об успехе или, в случае ошибки во входных данных,
        /// страницу с формой создания клиента с подсвеченными неверными полями.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> Create(NewIdSrvClientDto client)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(client);
            }

            bool created = await this.ClientService.CreateClientAsync(client);
            if (!created)
            {
                this.ModelState.AddModelError(string.Empty, "Такой клиент уже существует");
            }

            return created ? this.ViewSuccess("Клиент успешно создан") : this.View(client) as ActionResult;
        }

        /// <summary>
        /// Отобразить страницу обновлния данных клиента.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// View для отображения формы обновления данных клиента (существующие поля будут заполнены),
        /// либо переадресация на Index с сообещеним об ошибке, если клиента с таким id не существует.</returns>
        [HttpGet]
        public async Task<ActionResult> Update(Guid id)
        {
            IdSrvClientDto client = await this.ClientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return this.ViewError("Такого клиента не существует") as ActionResult;
            }

            var updateClient = new UpdateIdSrvClientDto
            {
                Id = client.Id,
                Name = client.Name,
                Secret = client.Secret,
                Uri = client.Uri,
            };
            return this.View(updateClient);
        }

        /// <summary>
        /// Обновить данные клиент.
        /// </summary>
        /// <param name="client">Данные для обновления.</param>
        /// <returns>
        /// В случае успешного обнолвения, переадресует на Index с сообщением об успехе,
        /// в противном случае снова отобразит форму обновления клиента с подсвеченными неправильными полями.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> Update(UpdateIdSrvClientDto client)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(client);
            }

            bool updated = await this.ClientService.UpdateClientAsync(client);
            if (!updated)
            {
                this.ModelState.AddModelError(string.Empty, "Не удалось обновить клиента");
            }

            return updated ? this.ViewSuccess("Клиент успешно обновлён") : this.View(client) as ActionResult;
        }

        /// <summary>
        /// Удалить клиента по его id.
        /// </summary>
        /// <param name="id">id клиента.</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Clients/Delete")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await this.ClientService.DeleteClientAsync(id);
            return deleted ? this.ViewSuccess("Клиент успешно удалён") : this.ViewError("Не удалось удалить клиента");
        }

        /// <summary>
        /// Заблокировать клиента по его id.
        /// </summary>
        /// <param name="id">id клиента</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Clients/Block")]
        public async Task<ActionResult> Block(Guid id)
        {
            bool blocked = await this.ClientService.ChangeBlock(new IdSrvClientBlockDto { Id = id, IsBlocked = true });
            return blocked ?
                this.ViewSuccess("Клиент заблокирован") :
                this.ViewError("Не удалось заблокировать клиента");
        }

        /// <summary>
        /// Разаблокировать клиента по его id.
        /// </summary>
        /// <param name="id">id клиента</param>
        /// <returns>
        /// Переадресует на Index с сообщением либо об успехе, либо о неудаче.
        /// </returns>
        [HttpPost]
        [Route("/Clients/Unblock")]
        public async Task<ActionResult> Unblock(Guid id)
        {
            bool unblocked = await this.ClientService.ChangeBlock(new IdSrvClientBlockDto { Id = id, IsBlocked = false });
            return unblocked ?
                this.ViewSuccess("Клиент разблокирован") :
                this.ViewError("Не удалось разблокировать клиента");
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об успехе.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением об успехе в TempData.
        /// </returns>
        private RedirectToRouteResult ViewSuccess(string message)
        {
            return this.ViewMessage(message, false);
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением об ошибке в TempData.
        /// </returns>
        private RedirectToRouteResult ViewError(string message)
        {
            return this.ViewMessage(message, true);
        }

        /// <summary>
        /// Переадресовать на Index с сообещнием об успехе или ошибке.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="isError">true, если это сообщение об ошибке.</param>
        /// <returns>
        /// Переадресация на Index с выставленным сообщением в секции _Message структуры TempData
        /// и его значением _IsError равным true или false (сообщение об ошибке или успехе соответственно).
        /// </returns>
        private RedirectToRouteResult ViewMessage(string message, bool isError)
        {
            this.TempData["_IsError"] = isError;
            this.TempData["_Message"] = message;
            return this.RedirectToAction(nameof(this.Index));
        }
    }
}