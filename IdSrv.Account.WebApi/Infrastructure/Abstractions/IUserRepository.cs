namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Threading.Tasks;

    public interface IUserRepository
    {
        Task<IdSrvUserDTO> GetByIdAsync(Guid id);

        Task<IdSrvUserDTO> GetByAuthIndoAsync(IdSrvUserAuthDTO userAuth);

        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user);

        Task<bool> DeleteAsync(Guid id);

        Task<RepositoryResponse> UpdateAsync(IdSrvUserDTO user);

        Task<bool> ChangePasswordAsync(IdSrvUserPasswordDTO password);
    }
}