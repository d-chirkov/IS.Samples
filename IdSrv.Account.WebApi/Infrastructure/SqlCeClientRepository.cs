namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using SqlKata.Compilers;
    using SqlKata.Execution;

    public class SqlCeClientRepository : IClientRepository
    {
        public SqlCeConnectionFactory DatabaseConnectionFactory { get; set; }

        public SqlCeClientRepository(SqlCeConnectionFactory connectionFactory)
        {
            this.DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<IdSrvClientDTO>> GetAllAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Select("Id", "Name", "Uri", "Secret", "IsBlocked")
                    .GetAsync<IdSrvClientDTO>();
            }
        }

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

        public async Task<IdSrvClientDTO> GetByIdAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Clients")
                    .Where(new { Id = id })
                    .Select("Id", "Name", "Uri", "Secret", "IsBlocked")
                    .FirstOrDefaultAsync<IdSrvClientDTO>();
            }
        }

        public async Task<IdSrvClientDTO> GetByNameAsync(string clientName)
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
                    .FirstOrDefaultAsync<IdSrvClientDTO>();
            }
        }

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

        public async Task<RepositoryResponse> CreateAsync(NewIdSrvClientDTO client)
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

        public async Task<RepositoryResponse> UpdateAsync(UpdateIdSrvClientDTO client)
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

        public async Task<RepositoryResponse> ChangeBlockingAsync(IdSrvClientBlockDTO block)
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