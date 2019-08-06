namespace IdSrv.Account.WebApi.Infrastructure.Exceptions
{
    using System;

    /// <summary>
    /// Исключение репозитория клиентов. Инициализируется кодом, использующим
    /// <see cref="Abstractions.IClientRepository"/> в случаях, когда репозиторий вернул
    /// неожиданный ответ. Сами реализации интерфейса <see cref="Abstractions.IClientRepository"/>
    /// данное исключение не инициализируют.
    /// </summary>
    public class ClientRepositoryException : Exception
    {
        /// <summary>
        /// Инициализирует исключение, вызывает конструктор базового исключение <see cref="Exception"/>
        /// без дополнительной логики.
        /// </summary>
        public ClientRepositoryException() : base()
        {
        }

        /// <summary>
        /// Инициализирует исключение с сообщением, вызывает конструктор базового исключение <see cref="Exception"/>
        /// с сообщением без дополнительной логики.
        /// </summary>
        /// <param name="message">Поясняющее сообщение для исключения.</param>
        public ClientRepositoryException(string message) : base(message)
        {
        }
    }
}