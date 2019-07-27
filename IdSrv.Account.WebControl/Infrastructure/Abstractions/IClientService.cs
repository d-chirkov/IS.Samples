namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IClientService
    {
        Task<IEnumerable<IdSrvClientDto>> GetClientsAsync();

        Task<IdSrvClientDto> GetClientByIdAsync(Guid id);

        Task<bool> CreateClientAsync(NewIdSrvClientDto newClient);

        Task<bool> UpdateClientAsync(UpdateIdSrvClientDto client);

        Task<bool> DeleteClientAsync(Guid id);

        Task<bool> ChangeBlock(IdSrvClientBlockDto block);
    }
}