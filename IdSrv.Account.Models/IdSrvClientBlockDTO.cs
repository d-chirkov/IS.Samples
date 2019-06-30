namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvClientBlockDTO
    {
        public Guid Id { get; set; }

        public bool IsBlocked { get; set; }
    }
}