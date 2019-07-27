namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserDto
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public bool IsBlocked { get; set; }
    }
}