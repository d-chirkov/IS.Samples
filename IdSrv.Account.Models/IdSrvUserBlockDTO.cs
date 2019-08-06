﻿namespace IdSrv.Account.Models
{
    using System;

    /// <summary>
    /// Статус блокировки пользователя по его идентификатору
    /// </summary>
    public class IdSrvUserBlockDto
    {
        /// <summary>
        /// Получает или задает идентификатор пользователя
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Получает или задает значение, показывающее, заблокирован ли пользователь.
        /// Заблокированные пользователи не могут войти со своим логином и паролем
        /// ни на один из клиентов (сайты и приложения), им выводится соответствующее сообщение.
        /// </summary>
        public bool IsBlocked { get; set; }
    }
}