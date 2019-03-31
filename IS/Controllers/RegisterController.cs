using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IS.DTO;

namespace IS.Controllers
{
    [Route("register")]
    public class RegisterController : ApiController
    {
        [HttpPost]
        [Route("user")]
        public void AddUser(NewUser newUser)
        {

        }
        
        [HttpPost]
        [Route("client")]
        public void AddClient([FromBody]string value)
        {
        }
    }
}
