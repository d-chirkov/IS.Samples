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

    public class UserController : ApiController
    {

        private IUserRepository UserRepository { get; set; }

        public UserController(IUserRepository userRepository)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        // GET api/values
        public async Task<IdSrvUserDTO> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task Create(NewIdSrvUserDTO user)
        {
            throw new NotImplementedException();
        }

        public async Task Update(IdSrvUserDTO user)
        {
            throw new NotImplementedException();
        }

        // POST api/values
        public async Task ChangePassword(IdSrvUserPasswordDTO password)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task Check(IdSrvUserAuthDTO authInfo)
        {


        }
    }
}
