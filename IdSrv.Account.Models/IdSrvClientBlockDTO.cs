namespace IdSrv.Account.Models
{
    using System;

    /// <summary>
    /// Содержит информацию о том, заблокирован ли клиент с заданным id.
    /// </summary>
    public class IdSrvClientBlockDto
    {
        /// <summary>
        /// Получает или задает идентификатор клиента.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, заблокирован ли клиент.
        /// Если клиент заблокирован, то он не сможет пользоваться
        /// учётными записями idenstity server.
        /// </summary>
        public bool IsBlocked { get; set; }
    }
}