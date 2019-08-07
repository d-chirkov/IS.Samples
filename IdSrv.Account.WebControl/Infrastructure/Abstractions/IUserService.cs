namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс для доступа к хранилищу пользователей identity server.
    /// Может быть реализован как rest-клиент.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Получить всех пользователей.
        /// </summary>
        /// <returns>
        /// Перечисление пользователей, если данные успешно были получены (может быть пустым в этом случае),
        /// либо null, если возникли ошибки при получении данных (для rest-сервиса это может быть
        /// когда http статус отличен от 200).
        /// </returns>
        Task<IEnumerable<IdSrvUserDto>> GetUsersAsync();

        /// <summary>
        /// Создать нового пользователя.
        /// </summary>
        /// <param name="newUser">Данные для нового пользователя.</param>
        /// <returns>
        /// true - если пользователь успешно был создан,
        /// false - если возникли ошибки при создании пользователя (для rest-сервиса - например когда
        /// http статус отличен от 200).
        /// </returns>
        Task<bool> CreateUserAsync(NewIdSrvUserDto newUser);

        /// <summary>
        /// Изменить пароль пользователя.
        /// </summary>
        /// <param name="passwords">Новый пароль пользователя (dto содержит id пользователя и новый пароль).</param>
        /// <returns>
        /// true - если пароль успешно был обновлён,
        /// false - если возникли ошибки при обновлении пароля (для rest-сервиса - например когда
        /// http статус отличен от 200).
        /// </returns>
        Task<bool> ChangePasswordForUserAsync(IdSrvUserPasswordDto passwords);

        /// <summary>
        /// Удалить пользователя.
        /// </summary>
        /// <param name="id">id удаляемого пользователя.</param>
        /// <returns>
        /// true - если пользователь успешно был удалён,
        /// false - если возникли ошибки при удалении пользователя (для rest-сервиса - например
        /// когда http статус отличен от 200).
        /// </returns>
        Task<bool> DeleteUserAsync(Guid id);

        /// <summary>
        /// Изменить статус блокировки пользователя.
        /// </summary>
        /// <param name="block">Данные для блокироваки пользователя (его id и новый статус).</param>
        /// <returns>
        /// true - если статус блокировки пользователя успешно был изменён,
        /// false - если возникли ошибки при изменении статуса блокировки пользователя (для rest-сервиса - например
        /// когда http статус отличен от 200).
        /// </returns>
        Task<bool> ChangeBlock(IdSrvUserBlockDto block);
    }
}