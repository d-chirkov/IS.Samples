namespace IS.Api.Controllers
{
    using IS.DTO;
    using SharedLib.IS;
    using System.Web.Http;

    [RoutePrefix("api/register")]
    public class RegisterController : ApiController
    {
        [HttpPost]
        [Route("user")]
        public IHttpActionResult AddUser(NewUser newUser)
        {
            int? added = UserRepo.SetUser(newUser.Name, newUser.Password);
            if (added == null)
            {
                var user = UserRepo.GetUser(newUser.Name);
                return user == null ? 
                    (IHttpActionResult)NotFound() : 
                    Content(System.Net.HttpStatusCode.Conflict, new { id = user.Id, name = newUser.Name });
            }
            return Ok(new { id = added.Value, name = newUser.Name });
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
