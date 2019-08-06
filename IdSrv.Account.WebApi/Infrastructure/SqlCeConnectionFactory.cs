namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Threading.Tasks;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;

    /// <summary>
    /// Фабрика подключений к базе данных SqlCe.
    /// </summary>
    public class SqlCeConnectionFactory : IDatabaseConnectionFactory
    {
        /// <summary>
        /// Инициализирует фабрику подключений к базе данных SqlCe.
        /// </summary>
        /// <param name="connectionString">Строка подключения ADO.NET, указывается путь к файлу .sdf как "Data Source".</param>
        public SqlCeConnectionFactory(string connectionString)
        {
            this.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Получает или задает строку подключения ADO.NET, указывается путь к файлу .sdf как "Data Source".
        /// </summary>
        private string ConnectionString { get; set; }

        /// <summary>
        /// Получить новое подключение к базе данных.
        /// </summary>
        /// <returns>
        /// Новое открытое подключение к базе данных.
        /// Необходимо использователь в выражениях using, либо явно делать Close().
        /// </returns>
        public async Task<IDbConnection> GetConnectionAsync()
        {
            var connection = new SqlCeConnection(this.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}