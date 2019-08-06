namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс репозитория пользователей.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Получить всех пользователей.
        /// </summary>
        /// <returns>
        /// Список (возможно пустой, но не null) пользователей.
        /// </returns>
        Task<IEnumerable<IdSrvUserDto>> GetAllAsync();

        /// <summary>
        /// Получить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>
        /// Пользователь, либо null, если такой пользователь не найден.
        /// </returns>
        Task<IdSrvUserDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Получить пользователя по логину.
        /// </summary>
        /// <param name="userName">Логин пользователя.</param>
        /// <returns>
        /// Пользователь, либо null, если такой пользователь не найден.
        /// </returns>
        Task<IdSrvUserDto> GetByUserNameAsync(string userName);

        /// <summary>
        /// Получить пользователя по его аутентификационным данным (логину и паролю).
        /// Данные метод необходим для проверки аутентификационных данных.
        /// </summary>
        /// <param name="userAuth">Аутентификационные данные пользователя.</param>
        /// <returns>
        /// Пользователь, либо null, если такой пользователь не найден, т.е. логин и/или пароль не верные.
        /// </returns>
        Task<IdSrvUserDto> GetByAuthInfoAsync(IdSrvUserAuthDto userAuth);

        /// <summary>
        /// Создать пользователя.
        /// </summary>
        /// <param name="user">Данные нового пользователя.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если пользователь успешно создан;
        /// <see cref="RepositoryResponse.Conflict"/>, если пользователь с таким логином уже сущетсвует.
        /// </returns>
        Task<RepositoryResponse> CreateAsync(NewIdSrvUserDto user);

        /// <summary>
        /// Удалить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор пользователя.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если пользователь удалён;
        /// <see cref="RepositoryResponse.NotFound"/>, если пользователь с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> DeleteAsync(Guid id);

        /// <summary>
        /// Изменить пароль пользователя.
        /// </summary>
        /// <param name="password">Dto с идентификатором пользователя и новым паролем для него.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если пароль изменён;
        /// <see cref="RepositoryResponse.NotFound"/>, если пользователь с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDto password);

        /// <summary>
        /// Изменить статус блокировки пользователя.
        /// </summary>
        /// <param name="block">Новый статус блокировки пользователя.</param>
        /// <returns>
        /// <see cref="RepositoryResponse.Success"/>, если статус блокировки пользователя успешно обновлён;
        /// <see cref="RepositoryResponse.NotFound"/>, если пользователь с таким id не найден.
        /// </returns>
        Task<RepositoryResponse> ChangeBlockingAsync(IdSrvUserBlockDto block);
    }
}