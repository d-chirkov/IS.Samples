namespace IdSrv.Account.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Пароль пользователя по идентификатору пользователя.
    /// </summary>
    public class IdSrvUserPasswordDto
    {
        /// <summary>
        /// Получает или задает идентификатор пользователя.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Получает или задает пароль пользователя.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}