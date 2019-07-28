namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserBlockDto
    {
        public Guid Id { get; set; }

        public bool IsBlocked{ get; set; }
    }
}