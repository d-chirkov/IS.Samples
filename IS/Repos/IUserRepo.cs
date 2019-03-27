using IS.Models;

namespace IS.Repos
{
    public interface IUserRepo
    {
        User GetUser(string name, string password);
        User GetUser(int id);
        bool SetUser(string name, string password);
    }
}
