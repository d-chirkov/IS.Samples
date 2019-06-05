namespace IdSrv.Account.WebControl.Models
{
    using System;

    public class IdSrvApplicationDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }

        public string Uri { get; set; }

        public bool Enabled { get; set; }
    }
}