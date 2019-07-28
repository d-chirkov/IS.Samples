namespace IdSrv.Server.Repositories.Abstractions
{
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    internal interface IUserRepository
    {
        Task<IdSrvUserDto> GetUserByUserNameAndPasswordAsync(string userName, string password);

        Task<IdSrvUserDto> GetUserByIdAsync(string id);

        Task<IdSrvUserDto> GetUserByUserNameAsync(string userName);
    }
}
