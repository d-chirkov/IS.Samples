namespace IdSrv.Account.Models
{
    using System;

    /// <summary>
    /// Содержит всю информацию о клиенте.
    /// </summary>
    public class IdSrvClientDto
    {
        /// <summary>
        /// Получает или задает идентификатор клиента в базе данных.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Получает или задает имя клиента, оно нужно для его идентификации
        /// и используется при настройке клиентов на использование identity server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Получает или задает секрет клиента, любой набор символов.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Получает или задает uri-адрес пользователя, может быть null в случае, когда клиент
        /// является настольным приложением. Для клиентов-сайтов указывается их адрес. На этот
        /// адрес будет редиректить обратно с identity server после входа пользователя.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, заблокирован ли клиент.
        /// Если клиент заблокирован, то он не сможет пользоваться
        /// учётными записями idenstity server.
        /// </summary>
        public bool IsBlocked { get; set; }
    }
}