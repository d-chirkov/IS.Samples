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

    [RoutePrefix("Api/User")]
    public class UserController : ApiController
    {
        private IUserRepository UserRepository { get; set; }

        public UserController(IUserRepository userRepository)
        {
            this.UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IHttpActionResult> GetAll()
        {
            IEnumerable<IdSrvUserDto> users = await this.UserRepository.GetAllAsync();
            return users != null ? this.Ok(users) : this.NotFound() as IHttpActionResult;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvUserDto user = await this.UserRepository.GetByIdAsync(id);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        [HttpGet]
        [Route("GetByUserName")]
        public async Task<IHttpActionResult> GetByUserName(string userName)
        {
            IdSrvUserDto user = await this.UserRepository.GetByUserNameAsync(userName);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        [HttpPost]
        [Route("GetByAuthInfo")]
        public async Task<IHttpActionResult> GetByAuthInfo(IdSrvUserAuthDto authInfo)
        {
            // This action check credentials only for simple users, not windows users.
            // So it's necessary to get password from client
            if (authInfo == null || authInfo.UserName == null || authInfo.Password == null)
            {
                return this.BadRequest();
            }

            IdSrvUserDto user = await this.UserRepository.GetByAuthInfoAsync(authInfo);
            return user != null ? this.Ok(user) : this.NotFound() as IHttpActionResult;
        }

        [HttpPut]
        public async Task<IHttpActionResult> Create(NewIdSrvUserDto user)
        {
            // Password can be null for windows users
            if (user == null || user.UserName == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.CreateAsync(user);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.Conflict ? this.Conflict() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(IdSrvUserPasswordDto password)
        {
            if (password == null || password.Password == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.ChangePasswordAsync(password);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpPost]
        [Route("ChangeBlocking")]
        public async Task<IHttpActionResult> ChangeBlocking(IdSrvUserBlockDto block)
        {
            if (block == null)
            {
                return this.BadRequest();
            }

            RepositoryResponse response = await this.UserRepository.ChangeBlockingAsync(block);
            return
                response == RepositoryResponse.Success ? this.Ok() :
                response == RepositoryResponse.NotFound ? this.NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        [HttpDelete]
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
