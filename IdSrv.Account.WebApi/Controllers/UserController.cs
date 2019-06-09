namespace IdSrv.Account.WebApi.Controllers
{
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using IdSrv.Account.Models;
    using System.Threading.Tasks;
    using IdSrv.Account.WebApi.Infrastructure;
    using IdSrv.Account.WebApi.Infrastructure.Exceptions;

    public class UserController : ApiController
    {

        private IUserRepository UserRepository { get; set; }

        public UserController(IUserRepository userRepository)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IHttpActionResult> Get(Guid id)
        {
            IdSrvUserDTO user = await this.UserRepository.GetByIdAsync(id);
            return user != null ? Ok(user) : NotFound() as IHttpActionResult;
        }

        public async Task<IHttpActionResult> Create(NewIdSrvUserDTO user)
        {
            RepositoryResponse response = await this.UserRepository.CreateAsync(user);
            return 
                response == RepositoryResponse.Success ? Ok() :
                response == RepositoryResponse.Conflict ? Conflict() as IHttpActionResult:
                throw new UserRepositoryException();
        }

        public async Task<IHttpActionResult> Update(IdSrvUserDTO user)
        {
            RepositoryResponse response = await this.UserRepository.UpdateAsync(user);
            return
                response == RepositoryResponse.Success ? Ok() :
                response == RepositoryResponse.Conflict ? Conflict() :
                response == RepositoryResponse.NotFound ? NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        public async Task<IHttpActionResult> ChangePassword(IdSrvUserPasswordDTO password)
        {
            RepositoryResponse response = await this.UserRepository.ChangePasswordAsync(password);
            return
                response == RepositoryResponse.Success ? Ok() :
                response == RepositoryResponse.NotFound ? NotFound() as IHttpActionResult :
                throw new UserRepositoryException();
        }

        public async Task<IHttpActionResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IHttpActionResult> Check(IdSrvUserAuthDTO authInfo)
        {
            throw new NotImplementedException();
        }
    }
}
