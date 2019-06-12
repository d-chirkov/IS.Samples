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

        public async Task<IdSrvUserDTO> GetByIdAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Users")
                    .Select("Id", "UserName")
                    .Where(new { Id = id })
                    .FirstOrDefaultAsync<IdSrvUserDTO>();
            }
        }

        public async Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth)
        {
            if (userAuth == null || userAuth.UserName == null || userAuth.Password == null)
            {
                throw new ArgumentNullException(nameof(userAuth));
            }
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                dynamic userInDb = await db
                    .Query("Users")
                    .Select("Id", "UserName", "PasswordHash", "PasswordSalt")
                    .Where(new { UserName = userAuth.UserName })
                    .FirstOrDefaultAsync();
                if (userInDb == null)
                {
                    return null;
                }
                string passwordHashFromDb = userInDb.PasswordHash;
                string passwordSaltFromDb = userInDb.PasswordSalt;
                string calculatedPasswordHash = this.GetB64PasswordHashFrom(userAuth.Password, passwordSaltFromDb);
                return calculatedPasswordHash == passwordHashFromDb ?
                    new IdSrvUserDTO { Id = userInDb.Id, UserName = userInDb.UserName } :
                    null;
            }
        }

        public Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user)
        {
            if (user == null || user.UserName == null || user.Password == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                string passwordSalt = Guid.NewGuid().ToString();
                try
                {
                    int inserted = await db.Query("Users").InsertAsync(new
                    {
                        user.UserName,
                        PasswordHash = this.GetB64PasswordHashFrom(user.Password, passwordSalt),
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

        private string GetB64PasswordHashFrom(string password, string salt)
        {
            SHA512 sha512 = new SHA512Managed();
            byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
            return Convert.ToBase64String(rawPasswordHash);
        }

    }
}