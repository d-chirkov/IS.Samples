namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvClientBlockDto
    {
        public Guid Id { get; set; }

        public bool IsBlocked { get; set; }
    }
}