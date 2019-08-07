namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс для доступа к хранилищу клиентов identity server.
    /// Может быть реализован как rest-клиент.
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Получить всех клиентов.
        /// </summary>
        /// <returns>
        /// Перечисление клиентов, если данные успешно были получены (может быть пустым в этом случае),
        /// либо null, если возникли ошибки при получении данных (для rest-сервиса это может быть
        /// когда http статус отличен от 200).
        /// </returns>
        Task<IEnumerable<IdSrvClientDto>> GetClientsAsync();

        /// <summary>
        /// Получить данные клиента по его id.
        /// </summary>
        /// <param name="id">id получаемого клиента .</param>
        /// <returns>
        /// Данные клиента, если они успешно были получены,
        /// либо null, если возникли ошибки при получении данных (для rest-сервиса это может быть ошибка подключенияили
        /// или когда http статус отличен от 200).
        /// </returns>
        Task<IdSrvClientDto> GetClientByIdAsync(Guid id);

        /// <summary>
        /// Создать нового клиента.
        /// </summary>
        /// <param name="newClient">Данные для нового клиента.</param>
        /// <returns>
        /// true - если клиент успешно был создан,
        /// false - если возникли ошибки при создании клиента (для rest-сервиса - например когда http статус отличен от 200).
        /// </returns>
        Task<bool> CreateClientAsync(NewIdSrvClientDto newClient);

        /// <summary>
        /// Обновить данные существующего клиента.
        /// </summary>
        /// <param name="client">Данные для обновления клиента.</param>
        /// <returns>
        /// true - если клиент успешно был обновлён,
        /// false - если возникли ошибки при обновлении клиента (для rest-сервиса - например когда http статус отличен от 200).
        /// </returns>
        Task<bool> UpdateClientAsync(UpdateIdSrvClientDto client);

        /// <summary>
        /// Удалить клиента.
        /// </summary>
        /// <param name="id">id удаляемого клиента.</param>
        /// <returns>
        /// true - если клиент успешно был удалён,
        /// false - если возникли ошибки при удалении клиента (для rest-сервиса - например когда http статус отличен от 200).
        /// </returns>
        Task<bool> DeleteClientAsync(Guid id);

        /// <summary>
        /// Изменить статус блокировки клиента.
        /// </summary>
        /// <param name="block">Данные для блокироваки клиента (id клиента и новый статус).</param>
        /// <returns>
        /// true - если статус блокировки клиента успешно был изменён,
        /// false - если возникли ошибки при изменении статуса блокировки клиента (для rest-сервиса - например
        /// когда http статус отличен от 200).
        /// </returns>
        Task<bool> ChangeBlock(IdSrvClientBlockDto block);
    }
}