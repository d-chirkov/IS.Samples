namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<IEnumerable<IdSrvUserDTO>> GetUsersAsync();

        Task<bool> CreateUserAsync(NewIdSrvUserDTO newUser);

        Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDTO passwords);

        Task<bool> DeleteUserAsync(Guid id);

        Task<bool> ChangeBlock(IdSrvUserBlockDTO block);
    }
}