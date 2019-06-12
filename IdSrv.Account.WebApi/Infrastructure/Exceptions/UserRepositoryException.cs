namespace IdSrv.Account.WebApi.Infrastructure.Exceptions
{
    using System;

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