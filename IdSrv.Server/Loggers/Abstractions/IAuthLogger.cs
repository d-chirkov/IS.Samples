namespace IdSrv.Server.Loggers.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Интерфейс логгера, может иметь любую реализацию, в том числе использование 
    /// готовых решения для логирования.
    /// </summary>
    internal interface IAuthLogger
    {
        /// <summary>
        /// Записать в лог, что пользователь вошёл на один из сайтов
        /// (а значит и на все сайты) или в приложение.
        /// Подразумевается логирование именно операции входа, если пользователь
        /// вошёл на одном сайте, то логируется только эта операция. На остальных
        /// сайтах он уже будет вошедшим, т.е. посещение других сайтов не логируется.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="clientId">Id клиента.</param>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="clientName">Имя клиента.</param>
        /// <returns>Метод является асинхронным, возвращается задача с внутренним типом void.</returns>
        Task UserSignedInAsync(string userId, string clientId, string userName = null, string clientName = null);

        /// <summary>
        /// Записать в лог, что пользователь вышёл с одного из сайтов
        /// (а значит и со всез сайтов).
        /// При этом в лог записывается имя и идентификатор клиента, на котором
        /// ранее был произведён вход этого пользователя (что немного странно, но так).
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="clientId">Id клиента.</param>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="clientName">Имя клиента.</param>
        /// <returns>Метод является асинхронным, возвращается задача с внутренним типом void.</returns>
        Task UserSignedOutAsync(string userId, string clientId, string userName = null, string clientName = null);

        /// <summary>
        /// Клиент (сайт или приложение) запросил данные пользователя.
        /// Так, например, клиенты запрашивают у identity server информацию о том,
        /// заблокирован он или нет. Или набор набор claim-ов.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="clientId">Id клиента.</param>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="clientName">Имя клиента.</param>
        /// <param name="isSuccess">Были ли данные успешно переданы запросившему клиенту.</param>
        /// <returns>Метод является асинхронным, возвращается задача с внутренним типом void.</returns>
        Task ProfileDataAccessedAsync(string userId, string clientId, string userName = null, string clientName = null, bool isSuccess = false);

        /// <summary>
        /// Была произведена неудачная попытка входа (неверный пароль пользователя).
        /// </summary>
        /// <param name="userName">Логин пользователя.</param>
        /// <param name="clientId">Id клиента.</param>
        /// <param name="reason">Причина неуспешного входа.</param>
        /// <param name="clientName">Имя клиента.</param>
        /// <returns>Метод является асинхронным, возвращается задача с внутренним типом void.</returns>
        Task UnsuccessfulSigningInAsync(string userName, string clientId, string reason = null, string clientName = null);
    }
}
