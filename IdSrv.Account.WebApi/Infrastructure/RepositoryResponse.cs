using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Account.WebApi.Infrastructure
{
    public enum RepositoryResponse
    {
        Success,
        Conflict,
        NotFound
    }
}