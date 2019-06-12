namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using SqlKata.Compilers;
    using SqlKata.Execution;

    public class SqlCompactUserRepository : IUserRepository
    {
        public SqlCompactConnectionFactory DatabaseConnectionFactory { get; set; }

        public SqlCompactUserRepository(SqlCompactConnectionFactory connectionFactory)
        {
            this.DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<IdSrvUserDTO>> GetAllAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db.Query("Users").Select("Id", "UserName").GetAsync<IdSrvUserDTO>();
            }
        }

        public Task<IdSrvUserDTO> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                string passwordSalt = Guid.NewGuid().ToString();
                SHA512 sha512 = new SHA512Managed();
                byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user.Password + passwordSalt));
                string b64PasswordHash = Convert.ToBase64String(rawPasswordHash);
                try
                {
                    int inserted = await db.Query("Users").InsertAsync(new
                    {
                        user.UserName,
                        PasswordHash = b64PasswordHash,
                        PasswordSalt = passwordSalt
                    });
                    return inserted == 1 ? RepositoryResponse.Success : RepositoryResponse.Conflict;
                }
                catch (SqlCeException ex) when (ex.NativeError == 25016)
                {
                    return RepositoryResponse.Conflict;
                }
            }
        }

        public Task<RepositoryResponse> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<RepositoryResponse> UpdateAsync(IdSrvUserDTO user)
        {
            throw new NotImplementedException();
        }

    }
}