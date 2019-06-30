namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserBlockDTO
    {
        public Guid Id { get; set; }

        public bool IsBlocked{ get; set; }
    }
}