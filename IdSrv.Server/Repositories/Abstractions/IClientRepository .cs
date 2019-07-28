namespace IdSrv.Server.Repositories.Abstractions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    internal interface IClientRepository
    {
        Task<IdSrvClientDto> GetClientByIdAsync(string clientId);

        Task<IEnumerable<string>> GetAllUrisAsync();
    }
}
