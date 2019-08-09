namespace IdSrv.Server.Repositories.Abstractions
{
    using System.Threading.Tasks;
    using IdSrv.Account.Models;

    /// <summary>
    /// Интерфейс доступа к месту хранения данных пользователей. Реализацией может быть rest-клиент.
    /// Методы интерфейса могут возвращать null, это значит, что доступ к хранилищу есть, но в нём
    /// нет соответствующих данных или получить их не удаётся.
    /// </summary>
    internal interface IUserRepository
    {
        /// <summary>
        /// Получить данные пользователя по логину и паролю. Метод используется для проверки
        /// учёных данных при входе.
        /// </summary>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <returns>Данные пользователя если логин и пароль верные, иначе null.</returns>
        Task<IdSrvUserDto> GetUserByUserNameAndPasswordAsync(string userName, string password);

        /// <summary>
        /// Получить данные пользователя по его id.
        /// </summary>
        /// <param name="id">Id клиента.</param>
        /// <returns>Данные пользователя если пользователь с таким id сушетсвует, иначе null.</returns>
        Task<IdSrvUserDto> GetUserByIdAsync(string id);

        /// <summary>
        /// Получить данные пользователя по его логину.
        /// </summary>
        /// <param name="userName">Логин клиента.</param>
        /// <returns>Данные пользователя если пользователь с таким логином сушетсвует, иначе null.</returns>
        Task<IdSrvUserDto> GetUserByUserNameAsync(string userName);
    }
}
