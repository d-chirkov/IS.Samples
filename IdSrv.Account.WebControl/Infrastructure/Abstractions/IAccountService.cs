namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.WebControl.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAccountService
    {
        Task<IEnumerable<IdSrvUserDTO>> GetUsersAsync();

        Task<bool> CreateUserAsync(NewIdSrvUserDTO newUser);

        Task<bool> ChangePasswordForUserAsync(ChangeIdSrvUserPasswordDTO passwords);

        Task<IEnumerable<IdSrvApplicationDTO>> GetApplications();
    }
}