using System.ComponentModel.DataAnnotations;

namespace IdSrv.Account.Models
{
    public class NewIdSrvUserDto
    {
        [Required]
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}