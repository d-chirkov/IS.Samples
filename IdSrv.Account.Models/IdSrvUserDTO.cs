namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserDTO
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public bool IsBlocked { get; set; }
    }
}