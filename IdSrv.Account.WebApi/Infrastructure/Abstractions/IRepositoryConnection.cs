using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace IdSrv.Account.WebApi.Infrastructure.Abstractions
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> GetConnectionAsync();
    }
}