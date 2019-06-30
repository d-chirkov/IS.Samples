namespace IdSrv.Account.WebApi.Infrastructure.Exceptions
{
    using System;

    public class ClientRepositoryException : Exception
    {
        public ClientRepositoryException() : base()
        {
        }

        public ClientRepositoryException(string message) : base(message)
        {
        }
    }
}