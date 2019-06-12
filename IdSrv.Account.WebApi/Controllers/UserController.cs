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

    public class UserController : ApiController
    {
        private IUserRepository UserRepository { get; set; }

        public UserController(IUserRepository userRepository)
        {
            this.UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            IEnumerable<IdSrvUserDTO> users = await this.UserRepository.GetAll();
            return users != null ? this.Ok(users) : this.NotFound() as IHttpActionResult;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvUserDTO user = await this.UserRepository.GetByIdAsync(id);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetByAuthInfo(IdSrvUserAuthDTO authInfo)
        {
            IdSrvUserDTO user = await this.UserRepository.GetByAuthInfoAsync(authInfo);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Create(NewIdSrvUserDTO user)
        {
            RepositoryResponse response = await this.UserRepository.CreateAsync(user);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.Conflict ? this.Conflict() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Update(IdSrvUserDTO user)
        {
            RepositoryResponse response = await this.UserRepository.UpdateAsync(user);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.Conflict ? this.Conflict() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(IdSrvUserPasswordDTO password)
        {
            RepositoryResponse response = await this.UserRepository.ChangePasswordAsync(password);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            RepositoryResponse response = await this.UserRepository.DeleteAsync(id);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }
    }
}
