namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    public interface IClientRepository
    {
        Task<IEnumerable<IdSrvClientDTO>> GetAllAsync();

        Task<IEnumerable<string>> GetAllUrisAsync();

        Task<IdSrvClientDTO> GetByIdAsync(Guid id);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> CreateAsync(NewIdSrvClientDTO app);

        Task<RepositoryResponse> UpdateAsync(UpdateIdSrvClientDTO app);

        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvClientBlockDTO block);
    }
}