namespace IdSrv.Server.Repositories.Abstractions
{
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    internal interface IUserRepository
    {
        Task<IdSrvUserDTO> GetUserByUserNameAndPasswordAsync(string userName, string password);

        Task<IdSrvUserDTO> GetUserByIdAsync(string id);

        Task<IdSrvUserDTO> GetUserByUserNameAsync(string userName);
    }
}
