namespace IdSrv.Server.Repositories.Abstractions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс доступа к месту хранения данных клиентов. Реализацией может быть rest-клиент.
    /// Методы интерфейса могут возвращать null, это значит, что доступ к хранилищу есть, но в нём
    /// нет соответствующих данных или получить их не удаётся.
    /// </summary>
    internal interface IClientRepository
    {
        /// <summary>
        /// Получить клиента identity server по его идентификатору.
        /// </summary>
        /// <param name="clientId">Id клиента.</param>
        /// <returns>Данные клиента если клиент с таким id сушетсвует, иначе null.</returns>
        Task<IdSrvClientDto> GetClientByIdAsync(string clientId);

        /// <summary>
        /// Получить все не пустые uri клиентов. Для клиентов-приложений (wpf, winforms)
        /// такого uri нет, соответственно uri клиентов могут быть пустыми.
        /// </summary>
        /// <returns>URI клиентов в виде списка.</returns>
        Task<IEnumerable<string>> GetAllUrisAsync();
    }
}
