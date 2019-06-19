using System.ComponentModel.DataAnnotations;

namespace IdSrv.Account.Models
{
    public class NewIdSrvClientDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Secret { get; set; }

        public string Uri { get; set; }
    }
}