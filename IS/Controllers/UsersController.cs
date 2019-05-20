namespace IS
{
    using IS.ViewModels;
    using SharedLib.IS;
    using System.Linq;
    using System.Web.Mvc;

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