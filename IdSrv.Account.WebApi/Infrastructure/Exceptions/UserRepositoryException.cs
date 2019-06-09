using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Account.WebApi.Infrastructure.Exceptions
{
    public class UserRepositoryException : Exception
    {
        public UserRepositoryException() : base()
        {
        }

        public UserRepositoryException(string message) : base(message)
        {
        }
    }
}