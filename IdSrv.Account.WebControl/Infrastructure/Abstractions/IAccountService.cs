using IdSrv.Account.WebControl.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Account.WebControl.Infrastructure.Abstractions
{
    public interface IAccountService
    {
        IEnumerable<IdSrvUser> GetUsers();

        IEnumerable<IdSrvApplication> GetApplications();
    }
}