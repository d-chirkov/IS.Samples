namespace IdSrv.Account.WebControl.Infrastructure.Entities
{
    using System;

    public class IdSrvUser
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public bool Enabled { get; set; }
    }
}