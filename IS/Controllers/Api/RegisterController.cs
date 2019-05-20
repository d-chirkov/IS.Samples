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
            string addedId = UserRepo.SetUser(newUser.Name, newUser.Password);
            if (addedId == null)
            {
                var user = UserRepo.GetUserByName(newUser.Name);
                return user == null ? 
                    (IHttpActionResult)NotFound() : 
                    Content(System.Net.HttpStatusCode.Conflict, new { id = user.Id, name = newUser.Name });
            }
            return Ok(new { id = addedId, name = newUser.Name });
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
