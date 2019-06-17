namespace IdSrv.Account.Models
{
    using System;

    public class UpdateIdSrvClientDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Secret { get; set; }

        public string Uri { get; set; }
    }
}