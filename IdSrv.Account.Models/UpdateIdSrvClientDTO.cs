namespace IdSrv.Account.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UpdateIdSrvClientDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Secret { get; set; }

        public string Uri { get; set; }
    }
}