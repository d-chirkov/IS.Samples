namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvClientDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }

        public string Uri { get; set; }

        public bool IsBlocked { get; set; }
    }
}