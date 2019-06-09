namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Threading.Tasks;

    public interface IUserRepository
    {
        Task<IdSrvUserDTO> GetByIdAsync(Guid id);

        Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth);

        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> UpdateAsync(IdSrvUserDTO user);

        Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password);
    }
}