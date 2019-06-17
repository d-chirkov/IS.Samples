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

    public class SqlCeUserRepository : IUserRepository
    {
        public SqlCeConnectionFactory DatabaseConnectionFactory { get; set; }

        public SqlCeUserRepository(SqlCeConnectionFactory connectionFactory)
        {
            this.DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<IdSrvUserDTO>> GetAllAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Users")
                    .Select("Id", "UserName", "IsBlocked")
                    .GetAsync<IdSrvUserDTO>();
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
                    .Select("Id", "UserName", "IsBlocked")
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
                    .Select("Id", "UserName", "PasswordHash", "PasswordSalt", "IsBlocked")
                    .Where(new { UserName = userAuth.UserName })
                    .FirstOrDefaultAsync();
                // If PasswordHash and PasswordSalt are null, then it means that it's windows user,
                // this repository have not responsibility to authenticate windows users, so we just return null.
                if (userInDb == null || userInDb.PasswordHash == null)
                {
                    return null;
                }
                string passwordHashFromDb = userInDb.PasswordHash;
                string passwordSaltFromDb = userInDb.PasswordSalt;
                string calculatedPasswordHash = this.GetB64PasswordHashFrom(userAuth.Password, passwordSaltFromDb);
                return calculatedPasswordHash == passwordHashFromDb ?
                    new IdSrvUserDTO { Id = userInDb.Id, UserName = userInDb.UserName, IsBlocked = userInDb.IsBlocked } :
                    null;
            }
        }

        public async Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password)
        {
            if (password == null || password.Password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                string passwordSalt = Guid.NewGuid().ToString();
                int updated = await db
                    .Query("Users")
                    .Where(new { Id = password.UserId })
                    .WhereNotNull("PasswordHash")
                    .UpdateAsync(new
                    {
                        PasswordHash = this.GetB64PasswordHashFrom(password.Password, passwordSalt),
                        PasswordSalt = passwordSalt
                    });
                return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        public async Task<RepositoryResponse> ChangeBlockingAsync(IdSrvUserBlockDTO block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                int updated = await db
                    .Query("Users")
                    .Where(new { Id = block.UserId })
                    .UpdateAsync(new { block.IsBlocked });
                return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        public async Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user)
        {
            if (user == null || user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                string passwordSalt = user.Password != null ? Guid.NewGuid().ToString() : null;
                try
                {
                    int inserted = await db.Query("Users").InsertAsync(new
                    {
                        user.UserName,
                        PasswordHash = this.GetB64PasswordHashFrom(user.Password, passwordSalt),
                        PasswordSalt = passwordSalt,
                        IsBlocked = false
                    });
                    return inserted == 1 ? RepositoryResponse.Success : RepositoryResponse.Conflict;
                }
                catch (SqlCeException ex) when (ex.NativeError == 25016)
                {
                    return RepositoryResponse.Conflict;
                }
            }
        }

        public async Task<RepositoryResponse> DeleteAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                string passwordSalt = Guid.NewGuid().ToString();
                int deleted = await db
                    .Query("Users")
                    .Where(new { Id = id })
                    .DeleteAsync();
                return deleted == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        private string GetB64PasswordHashFrom(string password, string salt)
        {
            if (password == null || salt == null)
            {
                return null;
            }
            SHA512 sha512 = new SHA512Managed();
            byte[] rawPasswordHash = sha512.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
            return Convert.ToBase64String(rawPasswordHash);
        }

    }
}