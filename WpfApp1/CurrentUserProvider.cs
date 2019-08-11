namespace WpfApp1
{
    /// <summary>
    /// Место для хранения глобальных для всего приложения переменных.
    /// </summary>
    internal static class CurrentUserProvider
    {
        /// <summary>
        /// Получает или задает логин пользователя, вошедшего в приложение.
        /// </summary>
        public static string UserName { get; set; } = null;
    }
}
