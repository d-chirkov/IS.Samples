namespace Site1.Mvc5.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Форма регистрации нового пользователя (одновременно локально и в identity server).
    /// Служит для передачи данных между view и контроллером.
    /// </summary>
    public class RegisterForm
    {
        /// <summary>
        /// Получает или задает логин создаваемого пользователя.
        /// </summary>
        [Required]
        public string Login { get; set; }

        /// <summary>
        /// Получает или задает пароль создаваемого пользователя.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}