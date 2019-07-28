namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    public interface IClientRepository
    {
        Task<IEnumerable<IdSrvClientDto>> GetAllAsync();

        Task<IEnumerable<string>> GetAllUrisAsync();

        Task<IdSrvClientDto> GetByIdAsync(Guid id);

        Task<IdSrvClientDto> GetByNameAsync(string clientName);

        Task<RepositoryResponse> DeleteAsync(Guid id);

        Task<RepositoryResponse> CreateAsync(NewIdSrvClientDto app);

        Task<RepositoryResponse> UpdateAsync(UpdateIdSrvClientDto app);

        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvClientBlockDto block);
    }
}