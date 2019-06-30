namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Threading.Tasks;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;

    public class SqlCeConnectionFactory : IDatabaseConnectionFactory
    {
        private string ConnectionString { get; set; }

        public SqlCeConnectionFactory(string connectionString)
        {
            this.ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<IDbConnection> GetConnectionAsync()
        {
            var connection = new SqlCeConnection(this.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}