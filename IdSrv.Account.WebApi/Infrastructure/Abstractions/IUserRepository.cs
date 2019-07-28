namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    public interface IUserRepository
    {
        Task<IEnumerable<IdSrvUserDto>> GetAllAsync();

        Task<IdSrvUserDto> GetByIdAsync(Guid id);

        Task<IdSrvUserDto> GetByUserNameAsync(string userName);

        Task<IdSrvUserDto> GetByAuthInfoAsync(IdSrvUserAuthDto userAuth);

        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDto user);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDto password);

        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvUserBlockDto block);
    }
}