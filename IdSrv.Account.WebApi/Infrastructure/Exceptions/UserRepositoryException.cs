namespace IdSrv.Account.WebApi.Infrastructure.Exceptions
{
    using System;

    /// <summary>
    /// Исключение репозитория пользователей. Инициализируется кодом, использующим
    /// <see cref="Abstractions.IUserRepository"/> в случаях, когда репозиторий вернул
    /// неожиданный ответ. Сами реализации интерфейса <see cref="Abstractions.IUserRepository"/>
    /// данное исключение не инициализируют.
    /// </summary>
    public class UserRepositoryException : Exception
    {
        /// <summary>
        /// Инициализирует исключение, вызывает конструктор базового исключение <see cref="Exception"/>
        /// без дополнительной логики.
        /// </summary>
        public UserRepositoryException() : base()
        {
        }

        /// <summary>
        /// Инициализирует исключение с сообщением, вызывает конструктор базового исключение <see cref="Exception"/>
        /// с сообщением без дополнительной логики.
        /// </summary>
        /// <param name="message">Поясняющее сообщение для исключения.</param>
        public UserRepositoryException(string message) : base(message)
        {
        }
    }
}