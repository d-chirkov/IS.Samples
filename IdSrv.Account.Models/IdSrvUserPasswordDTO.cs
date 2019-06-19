namespace IdSrv.Account.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class IdSrvUserPasswordDTO
    {
        public Guid UserId { get; set; }

        [Required]
        public string Password { get; set; }
    }
}