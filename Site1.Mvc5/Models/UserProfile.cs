namespace Site1.Mvc5.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Отражает профиль пользователя в локальной БД, необходимо для работы Entity Framework.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Получает или задает идентификатор пользователя в локальной БД (не в identity server).
        /// Является первичным ключом в БД.
        /// </summary>
        [Key]
        public int UserProfileId { get; set; }

        /// <summary>
        /// Получает или задает идентификатор пользователя в identity server.
        /// </summary>
        public string IdSrvId { get; set; }

        /// <summary>
        /// Получает или задает логин пользователя.
        /// </summary>
        public string Login { get; set; }
    }
}