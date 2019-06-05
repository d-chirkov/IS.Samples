namespace IdSrv.Account.WebControl.Infrastructure.Entities
{
    using System;

    public class IdSrvApplication
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }

        public string Uri { get; set; }

        public bool Enabled { get; set; }
    }
}