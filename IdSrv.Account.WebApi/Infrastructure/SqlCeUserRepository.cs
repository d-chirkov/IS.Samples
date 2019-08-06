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

    /// <summary>
    /// Реализация <see cref="IUserRepository"/> для работы с SqlCe.
    /// </summary>
    public class SqlCeUserRepository : IUserRepository
    {
        /// <summary>
        /// Инициализирует репозиторий.
        /// </summary>
        /// <param name="connectionFactory">Фабрика подключний к базе данных SqlCe.</param>
        public SqlCeUserRepository(SqlCeConnectionFactory connectionFactory)
        {
            this.DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Получает или задает фабрику подключений к базе данных SqlCe.
        /// </summary>
        public SqlCeConnectionFactory DatabaseConnectionFactory { get; set; }

        /// <inheritdoc/>
        public async Task<IEnumerable<IdSrvUserDto>> GetAllAsync()
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Users")
                    .Select("Id", "UserName", "IsBlocked")
                    .GetAsync<IdSrvUserDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetByIdAsync(Guid id)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Users")
                    .Select("Id", "UserName", "IsBlocked")
                    .Where(new { Id = id })
                    .FirstOrDefaultAsync<IdSrvUserDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetByUserNameAsync(string userName)
        {
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var compiler = new SqlServerCompiler();
                var db = new QueryFactory(connection, compiler);
                return await db
                    .Query("Users")
                    .Select("Id", "UserName", "IsBlocked")
                    .Where(new { UserName = userName })
                    .FirstOrDefaultAsync<IdSrvUserDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IdSrvUserDto> GetByAuthInfoAsync(IdSrvUserAuthDto userAuth)
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
                    new IdSrvUserDto { Id = userInDb.Id, UserName = userInDb.UserName, IsBlocked = userInDb.IsBlocked } :
                    null;
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDto password)
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
                    .Where(new { Id = password.Id })
                    .WhereNotNull("PasswordHash")
                    .UpdateAsync(new
                    {
                        PasswordHash = this.GetB64PasswordHashFrom(password.Password, passwordSalt),
                        PasswordSalt = passwordSalt,
                    });
                return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> ChangeBlockingAsync(IdSrvUserBlockDto block)
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
                    .Where(new { Id = block.Id })
                    .UpdateAsync(new { block.IsBlocked });
                return updated == 1 ? RepositoryResponse.Success : RepositoryResponse.NotFound;
            }
        }

        /// <inheritdoc/>
        public async Task<RepositoryResponse> CreateAsync(NewIdSrvUserDto user)
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
                        IsBlocked = false,
                    });
                    return inserted == 1 ? RepositoryResponse.Success : RepositoryResponse.Conflict;
                }
                catch (SqlCeException ex) when (ex.NativeError == 25016)
                {
                    return RepositoryResponse.Conflict;
                }
            }
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Получить представление Base64 от хэша SHA512 от конкатенации пароля с солью.
        /// </summary>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="salt">Соль для пароля.</param>
        /// <returns>
        /// Представление Base64 от хэша SHA512 от конкатенации пароля с солью,
        /// либо null, если либо пароль, либо соль равены null.
        /// </returns>
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