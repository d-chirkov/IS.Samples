namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserBlockDTO
    {
        public Guid UserId { get; set; }

        public bool IsBlocked{ get; set; }
    }
}