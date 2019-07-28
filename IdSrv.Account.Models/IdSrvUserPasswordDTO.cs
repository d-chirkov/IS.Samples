namespace IdSrv.Account.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class IdSrvUserPasswordDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Password { get; set; }
    }
}