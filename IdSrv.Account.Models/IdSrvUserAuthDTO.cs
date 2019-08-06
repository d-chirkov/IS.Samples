namespace IdSrv.Account.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Представляет учётные данные пользователя identity server (логин и пароль)
    /// </summary>
    public class IdSrvUserAuthDto
    {
        /// <summary>
        /// Получает или задает логин пользователя.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Получает или задает пароль пользователя.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
