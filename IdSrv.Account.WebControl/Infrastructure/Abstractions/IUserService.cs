namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<IEnumerable<IdSrvUserDto>> GetUsersAsync();

        Task<bool> CreateUserAsync(NewIdSrvUserDto newUser);

        Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDto passwords);

        Task<bool> DeleteUserAsync(Guid id);

        Task<bool> ChangeBlock(IdSrvUserBlockDto block);
    }
}