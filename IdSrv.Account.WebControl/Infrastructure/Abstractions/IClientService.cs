namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using IdSrv.Account.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IClientService
    {
        Task<IEnumerable<IdSrvClientDTO>> GetClientsAsync();

        Task<IdSrvClientDTO> GetClientByIdAsync(Guid id);

        Task<bool> CreateClientAsync(NewIdSrvClientDTO newClient);

        Task<bool> UpdateClientAsync(UpdateIdSrvClientDTO client);

        Task<bool> DeleteClientAsync(Guid id);

        Task<bool> ChangeBlock(IdSrvClientBlockDTO block);
    }
}