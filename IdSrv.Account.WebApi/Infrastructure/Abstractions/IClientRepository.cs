namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс репозитория клиентов.
    /// </summary>
    public interface IClientRepository
    {
        /// <summary>
        /// Получить всех клиентов.
        /// </summary>
        /// <returns>
        /// Список (возможно пустой, но не null) клиентов.
        /// </returns>
        Task<IEnumerable<IdSrvClientDto>> GetAllAsync();

        /// <summary>
        /// Получить все uri всех клиентов.
        /// </summary>
        /// <returns>
        /// Список (возможно пустой, но не null) uri в виде строк.
        /// </returns>
        Task<IEnumerable<string>> GetAllUrisAsync();

        /// <summary>
        /// Получить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// Клиент, либо null, если такой клиент не найден.
        /// </returns>
        Task<IdSrvClientDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Получить клиента по имени.
        /// </summary>
        /// <param name="clientName">Имя клиента.</param>
        /// <returns>
        /// Клиент, либо null, если такой клиент не найден.
        /// </returns>
        Task<IdSrvClientDto> GetByNameAsync(string clientName);

        /// <summary>
        /// Удалить клиента по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если клиент удалён;
        /// <see cref="RepositoryResponse.NotFound"/>, если клиент с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> DeleteAsync(Guid id);

        /// <summary>
        /// Создать клиента.
        /// </summary>
        /// <param name="app">Данные нового клиента.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если клиент успешно создан;
        /// <see cref="RepositoryResponse.Conflict"/>, если клиент с таким именем уже сущетсвует.
        /// </returns>
        Task<RepositoryResponse> CreateAsync(NewIdSrvClientDto app);

        /// <summary>
        /// Обновить данные клиента.
        /// </summary>
        /// <param name="app">Новые данные обновляемого клиента.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если клиент успешно обновлён;
        /// <see cref="RepositoryResponse.Conflict"/>, если новые данные конфликтуют с уже существующим клиентом (имя клиента);
        /// <see cref="RepositoryResponse.NotFound"/>, если клиент с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> UpdateAsync(UpdateIdSrvClientDto app);

        /// <summary>
        /// Изменить статус блокировки клиента.
        /// </summary>
        /// <param name="block">Новый статус блокировки клиента.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если статус блокировки клиента успешно обновлён;
        /// <see cref="RepositoryResponse.NotFound"/>, если клиент с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvClientBlockDto block);
    }
}