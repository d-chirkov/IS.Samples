namespace Site1.Mvc5.Models
{
    using System.Data.Entity;

    /// <summary>
    /// Контекст аккаунтов для Entity Framework.
    /// </summary>
    public class AccountsContext : DbContext
    {
        /// <summary>
        /// Стандартный конструктор контекста.
        /// </summary>
        public AccountsContext() : base("AccountsDbContext")
        {
        }
        
        /// <summary>
        /// Получает или задает профили пользователей в локальной БД.
        /// </summary>
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}