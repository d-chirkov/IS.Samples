namespace IdSrv.Account.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;

    /// <summary>
    /// Контроллер для управления клиентами identity server-а.
    /// </summary>
    [RoutePrefix("Api/Client")]
    public class ClientController : ApiController
    {
        /// <summary>
        /// Создаёт экземпляр контроллера.
        /// </summary>
        /// <param name="clientRepository">Репозиторий клиентов.</param>
        public ClientController(IClientRepository clientRepository)
        {
            this.ClientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        /// <summary>
        /// Получает или задает репозиторий клиентов.
        /// </summary>
        private IClientRepository ClientRepository { get; set; }

        /// <summary>
        /// Получить всех клиентов.
        /// </summary>
        /// <returns>
        /// Ok со списком объектов <see cref="IdSrvClientDto"/>,
        /// либо NotFound, если получить такой список из репозитория не удалось.
        /// </returns>
        [HttpGet]
        [Route("GetAll")]
        public async Task<IHttpActionResult> GetAll()
        {
            IEnumerable<IdSrvClientDto> clients = await this.ClientRepository.GetAllAsync();
            return clients != null ? this.Ok(clients) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить список всех uri клиентов.
        /// </summary>
        /// <returns>
        /// Ok со списков объектов uri в виде строк,
        /// либо NotFound, если получить такой список из репозитория не удалось.
        /// </returns>
        [HttpGet]
        [Route("GetAllUris")]
        public async Task<IHttpActionResult> GetAllUris()
        {
            IEnumerable<string> uris = await this.ClientRepository.GetAllUrisAsync();
            return uris != null ? this.Ok(uris) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// Ok с dto типа <see cref="IdSrvClientDto"/>,
        /// либо NotFound, если такой клиент не найден в репозитории.
        /// </returns>
        [HttpGet]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvClientDto client = await this.ClientRepository.GetByIdAsync(id);
            return client != null ? this.Ok(client) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Получить клиента по его имени.
        /// </summary>
        /// <param name="name">Имя клиента.</param>
        /// <returns>
        /// Ok с dto типа <see cref="IdSrvClientDto"/>,
        /// либо NotFound, если такой клиент не найден в репозитории.
        /// </returns>
        [HttpGet]
        [Route("GetByName")]
        public async Task<IHttpActionResult> GetByName(string name)
        {
            if (name == null)
            {
                return this.BadRequest();
            }

            IdSrvClientDto client = await this.ClientRepository.GetByNameAsync(name);
            return client != null ? this.Ok(client) : this.NotFound() as IHttpActionResult;
        }

        /// <summary>
        /// Создать клиента в репозитории.
        /// </summary>
        /// <param name="client">Dto с данными для создания клиента.</param>
        /// <returns>
        /// Ok, если клиент успешно создан, Conflict в противном случае.
        /// Если клиента не удалось создать, то это означает, что клиент с такими данными (именем)
        /// уже существует в репозитории.
        /// </returns>
        [HttpPut]
        public async Task<IHttpActionResult> Create(NewIdSrvClientDto client)
        {
            if (client == null || client.Name == null || client.Secret == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.ClientRepository.CreateAsync(client);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.Conflict ? this.Conflict() as IHttpActionResult :
                throw new ClientRepositoryException();
        }

        /// <summary>
        /// Обновить информаци о клиенте.
        /// </summary>
        /// <param name="client">Dto с данными об обновлении</param>
        /// <returns>
        /// Ok, если данные успешно обновлены в репозитории;
        /// NotFound, если обновлемый клиент не найден в репозитории;
        /// Conflict, если новые данные конфликтуют с уже существующим клиентом (имя клиента).
        /// </returns>
        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(UpdateIdSrvClientDto client)
        {
            if (client == null || client.Name == null || client.Secret == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.ClientRepository.UpdateAsync(client);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() :
                response == RepositoryResponse.Conflict ? this.Conflict() as IHttpActionResult :
                throw new ClientRepositoryException();
        }

        /// <summary>
        /// Изменить статус блокировки клиента.
        /// </summary>
        /// <param name="block">Dto с новым статусом блокироваки.</param>
        /// <returns>
        /// Ok, если статус блокироваки изменён в репозитории;
        /// NotFound, если такого клиента не найдено.
        /// </returns>
        [HttpPost]
        [Route("ChangeBlocking")]
        public async Task<IHttpActionResult> ChangeBlocking(IdSrvClientBlockDto block)
        {
            if (block == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.ClientRepository.ChangeBlockingAsync(block);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new ClientRepositoryException();
        }

        /// <summary>
        /// Удалить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// Ok, если клиент успешно удалён из репозитория;
        /// NotFound, если клиент с таким идентификатором не найден.
        /// </returns>
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            RepositoryResponse response = await this.ClientRepository.DeleteAsync(id);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new ClientRepositoryException();
        }
    }
}
