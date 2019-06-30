namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    using System.Data;
    using System.Threading.Tasks;

    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> GetConnectionAsync();
    }
}