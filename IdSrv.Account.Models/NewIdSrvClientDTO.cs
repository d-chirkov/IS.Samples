namespace IdSrv.Account.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Данные для создания нового клиента identity server-а.
    /// </summary>
    public class NewIdSrvClientDto
    {
        /// <summary>
        /// Получает или задает имя клиента, оно нужно для его идентификации
        /// и используется при настройке клиентов на использование identity server.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Получает или задает секрет клиента, любой набор символов.
        /// </summary>
        [Required]
        public string Secret { get; set; }

        /// <summary>
        /// Получает или задает uri-адрес пользователя, может быть null в случае, когда клиент
        /// является настольным приложением. Для клиентов-сайтов указывается их адрес. На этот
        /// адрес будет редиректить обратно с identity server после входа пользователя.
        /// </summary>
        public string Uri { get; set; }
    }
}