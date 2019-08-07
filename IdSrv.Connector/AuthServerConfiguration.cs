namespace IdSrv.Connector
{
    /// <summary>
    /// Временная конфигурация клиента identity server, необходима для передачи данных
    /// между вызвами методов fluent api.
    /// </summary>
    internal class AuthServerConfiguration
    {
        /// <summary>
        /// Получает или задает URI-адрес identity server
        /// </summary>
        public string IdSrvAddress { get; set; }

        /// <summary>
        /// Получает или задает идентификатор клиента в виде строки (должен быть по идее Guid-ом).
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Получает или задает секрет клиента
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Получает или задает URL-адрес самого клиента.
        /// </summary>
        public string OwnAddress { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, использует ли клиент WebForms.
        /// </summary>
        public bool UseWebForms { get; set; } = false;

        /// <summary>
        /// Получает значение, показывающее, что все необходимые поля для настройки клиента identity server-а установлены.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return
                    this.IdSrvAddress != null &&
                    this.ClientId != null &&
                    this.ClientSecret != null &&
                    this.OwnAddress != null;
            }
        }
    }
}
