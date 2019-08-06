namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Интерфейс фабрики подключений к базе данных.
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Получить новое подключение к базе данных.
        /// </summary>
        /// <returns>
        /// Подключение к базе данных.
        /// </returns>
        Task<IDbConnection> GetConnectionAsync();
    }
}