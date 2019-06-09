namespace IdSrv.Account.Models
{
    using System;

    public class IdSrvUserPasswordDTO
    {
        public Guid UserId { get; set; }

        public string Password { get; set; }
    }
}