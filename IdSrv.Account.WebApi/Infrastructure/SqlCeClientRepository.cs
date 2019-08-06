namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using SqlKata.Compilers;
    using SqlKata.Execution;

    /// <summary>
    /// Реализация <see cref="IClientRepository"/> для работы с SqlCe.
    /// </summary>
    public class SqlCeClientRepository : IClientRepository
    {
        /// <summary>
        /// Инициализирует репозиторий.
        /// </summary>
        /// <param name="connectionFactory">Фабрика подключний к базе данных SqlCe.</param>
        public SqlCeClientRepository(SqlCeConnectionFactory connectionFactory)
        {
            this.DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Получает или задает фабрику подключений к базе данных SqlCe.
        /// </summary>
        public SqlCeConnectionFactory DatabaseConnectionFactory { get; set; }

        /// <inheritdoc/>
        public async Task<IEnumerable<IdSrvClientDto>> GetAllAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Select("Id", "Name", "Uri", "Secret", "IsBlocked")
                    .GetAsync<IdSrvClientDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAllUrisAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Select("Uri")
                    .WhereNotNull("Uri")
                    .GetAsync<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<IdSrvClientDto> GetByIdAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Where(new { Id = id })
                    .Select("Id", "Name", "Uri", "Secret", "IsBlocked")
                    .FirstOrDefaultAsync<IdSrvClientDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IdSrvClientDto> GetByNameAsync(string clientName)
        {
            if (clientName == null)
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Where(new { Name = clientName })
                    .Select("Id", "Name", "Uri", "Secret", "IsBlocked")
                    .FirstOrDefaultAsync<IdSrvClientDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> DeleteAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                int deleted = await db.Query("Clients").Where(new { Id = id }).DeleteAsync();
                return deleted == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> CreateAsync(NewIdSrvClientDto client)
        {
            if (client == null || client.Name == null || client.Secret == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                try
                {
                    int inserted = await db.Query("Clients").InsertAsync(client);
                    return inserted == 1 ? RepositoryResponse.Success : RepositoryResponse.Conflict;
                }
                catch (SqlCeException ex) when (ex.NativeError == 25016)
                {
                    return RepositoryResponse.Conflict;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> UpdateAsync(UpdateIdSrvClientDto client)
        {
            if (client == null || client.Name == null || client.Secret == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                try
                {
                    int updated = await db.Query("Clients").Where(new { client.Id }).UpdateAsync(client);
                    return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
                }
                catch (SqlCeException ex) when (ex.NativeError == 25016)
                {
                    return RepositoryResponse.Conflict;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> ChangeBlockingAsync(IdSrvClientBlockDto block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                int updated = await db.Query("Clients").Where(new { block.Id }).UpdateAsync(block);
                return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }
    }
}