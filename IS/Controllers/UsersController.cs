using IS.Repos;
using IS.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace IS
{
    public class UsersController : Controller
    {
        public ActionResult GetAll()
        {
            var allUsers = new AllUsersViewModel
            {
                Users = UserRepo.GetAllUsers().Select(u => new UserViewModel { Id = u.Id, Name = u.Name })
            };
            return View(allUsers);
        }
    }
}