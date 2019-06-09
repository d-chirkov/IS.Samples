
namespace IdSrv.Account.WebApi.Infrastructure
{
    using System;
    using System.Data.SqlServerCe;
    using System.Threading.Tasks;
    using IdSrv.Account.Models;
    using IdSrv.Account.WebApi.Infrastructure.Abstractions;
    using SqlKata;
    using SqlKata.Execution;
    using SqlKata.Compilers;
    using System.Data.SqlServerCe;
    using System.Data;

    public class SqlCompactUserRepository : IUserRepository
    {
        public IDatabaseConnectionFactory DatabaseConnectionFactory { get; set; }

        public SqlCompactUserRepository(IDatabaseConnectionFactory connectionFactory)
        {
            DatabaseConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public Task<RepositoryResponse> ChangePasswordAsync(IdSrvUserPasswordDTO password)
        {
            throw new NotImplementedException();
        }

        public async Task<RepositoryResponse> CreateAsync(NewIdSrvUserDTO user)
        {
            var compiler = new SqlServerCompiler();
            using (IDbConnection connection = await this.DatabaseConnectionFactory.GetConnectionAsync())
            {
                var db = new QueryFactory(connection, compiler);
                while(true)
                {
                    var guid = Guid.NewGuid();
                    var foundById = await db.Query("Users").Select("Id").Where(new { Id = guid.ToString() }).FirstOrDefaultAsync();
                    if (foundById != null)
                    {
                        continue;
                    }
                    int inserted = await db.Query("Users").InsertAsync(new { Id = guid.ToString(), UserName = user.UserName, PasswordHash = user.Password + "abc", PasswordSalt = "abc" });
                    return inserted == 1 ? RepositoryResponse.Success : RepositoryResponse.Conflict;
                }
            }
        }

        public Task<RepositoryResponse> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IdSrvUserDTO> GetByAuthInfoAsync(IdSrvUserAuthDTO userAuth)
        {
            throw new NotImplementedException();
        }

        public Task<IdSrvUserDTO> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RepositoryResponse> UpdateAsync(IdSrvUserDTO user)
        {
            throw new NotImplementedException();
        }
    }
}