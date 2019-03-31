using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IS.DTO;
using IS.Repos;

namespace IS.Controllers
{
    [Route("register")]
    public class RegisterController : ApiController
    {
        [HttpPost]
        [Route("user")]
        public IHttpActionResult AddUser(NewUser newUser)
        {
            bool added = UserRepo.SetUser(newUser.Name, newUser.Password);
            return added ? (IHttpActionResult)Ok() : Conflict();
        }
        
        [HttpPost]
        [Route("client")]
        public IHttpActionResult AddClient(NewClient newClient)
        {
            bool added = ClientRepo.SetClient(newClient.Id, newClient.Name, newClient.Secret, newClient.Uri);
            return added ? (IHttpActionResult)Ok() : Conflict();
        }
    }
}
