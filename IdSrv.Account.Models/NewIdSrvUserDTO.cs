namespace IdSrv.Account.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Данные для создания нового пользователя identity server-а.
    /// </summary>
    public class NewIdSrvUserDto
    {
        /// <summary>
        /// Получает или задает логин пользователя, который вводится в форму входа.
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// Получает или задает пароль пользователя.
        /// </summary>
        public string Password { get; set; }
    }
}