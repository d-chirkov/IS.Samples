using System.ComponentModel.DataAnnotations;

namespace IdSrv.Account.Models
{
    public class NewIdSrvUserDTO
    {
        [Required]
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}