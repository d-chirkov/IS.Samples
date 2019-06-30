namespace IdSrv.Account.Models
{
    using System.ComponentModel.DataAnnotations;

    public class IdSrvUserAuthDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
