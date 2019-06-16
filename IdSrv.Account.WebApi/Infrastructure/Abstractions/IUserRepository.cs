namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    public interface IUserRepository
    {
        Task<IEnumerable<IdSrvUserDTO>> GetAllAsync();

        Task<IdSrvUserDTO> GetByIdAsync(Guid id);

        Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth);

        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password);

        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvUserBlockDTO block);
    }
}