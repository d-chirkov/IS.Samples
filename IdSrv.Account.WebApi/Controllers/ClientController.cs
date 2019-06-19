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

    [RoutePrefix("Api/Client")]
    public class ClientController : ApiController
    {
        private IClientRepository ClientRepository { get; set; }

        public ClientController(IClientRepository clientRepository)
        {
            this.ClientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IHttpActionResult> GetAll()
        {
            IEnumerable<IdSrvClientDTO> clients = await this.ClientRepository.GetAllAsync();
            return clients != null ? this.Ok(clients) : this.NotFound() as IHttpActionResult;
        }

        [HttpGet]
        [Route("GetAllUris")]
        public async Task<IHttpActionResult> GetAllUris()
        {
            IEnumerable<string> uris = await this.ClientRepository.GetAllUrisAsync();
            return uris != null ? this.Ok(uris) : this.NotFound() as IHttpActionResult;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvClientDTO client = await this.ClientRepository.GetByIdAsync(id);
            return client != null ? this.Ok(client) : this.NotFound() as IHttpActionResult;
        }

        [HttpPut]
        public async Task<IHttpActionResult> Create(NewIdSrvClientDTO client)
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

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(UpdateIdSrvClientDTO client)
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

        [HttpPost]
        [Route("ChangeBlocking")]
        public async Task<IHttpActionResult> ChangeBlocking(IdSrvClientBlockDTO block)
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
