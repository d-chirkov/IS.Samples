namespace IdSrv.Server.Loggers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IdSrv.Server.Loggers.Abstractions;

    /// <summary>
    /// Собственная простая реализация <see cref="IAuthLogger"/>, работающая с файлом
    /// и собственным простым форматом логирования. Должна быть заменена классом, использующим
    /// существующие библиотеки для логирования (на этапе разработки такой выбрано не было, надо
    /// было что-то тестовое подставить).
    /// Хотя реализуемый интерфейс <see cref="IAuthLogger"/> является асинхронным, все операции
    /// здесь на самом деле синхронные.
    /// </summary>
    public class FileAuthLogger : IAuthLogger
    {
        /// <summary>
        /// Инициирует логгер.
        /// </summary>
        /// <param name="pathToFile">
        /// Путь к файлу, в который надо писать лог.
        /// Файл не открывается при создании логгера и никак не проверяется доступность
        /// записи в него. Для каждой операции логирования файл открывается заново.
        /// </param>
        public FileAuthLogger(string pathToFile)
        {
            this.PathToFile = pathToFile ?? throw new ArgumentNullException(nameof(pathToFile));
        }

        /// <summary>
        /// Получает или задает путь к файлу, в который надо писать лог.
        /// </summary>
        private string PathToFile { get; set; }

        /// <inheritdoc/>
        public Task NotRegisteredUserTryToSignInAsync(string userName, string clientId, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [ERR] failed attempt to sign in, user not found";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task ProfileDataAccessedAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false)
        {
            string output = $"{this.GetTimeString()} [INF] access to user data";
            if (userId != null) output += $", user-id: {userId}" + (isBlocked ? " [BLOCKED]" : string.Empty);
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task UnsuccessfulSigningInAsync(string userName, string clientId, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [ERR] failed attempt to sign in, invalid credentials";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task UserSignedInAsync(string userId, string clientId, string userName = null, string clientName = null, bool isBlocked = false)
        {
            string output = $"{this.GetTimeString()} [INF] successufuly signed in";
            if (userId != null) output += $", user-id: {userId}" + (isBlocked ? " [BLOCKED]" : string.Empty);
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task UserSignedOutAsync(string userId, string clientId, string userName = null, string clientName = null)
        {
            string output = $"{this.GetTimeString()} [INF] signed out";
            if (userId != null) output += $", user-id: {userId}";
            if (userName != null) output += $", user-name: {userName}";
            if (clientId != null) output += $", client-id: {clientId}";
            if (clientName != null) output += $", client-name: {clientName}";
            File.AppendAllText(this.PathToFile, output + "\r\n");
            return Task.FromResult(0);
        }

        /// <summary>
        /// Получить строку с текущим временем.
        /// </summary>
        /// <returns>Строка с текцщим временем в формате yyyy-MM-dd HH:mm:ss.</returns>
        private string GetTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}