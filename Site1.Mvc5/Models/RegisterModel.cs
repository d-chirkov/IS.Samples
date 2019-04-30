using System.ComponentModel.DataAnnotations;

namespace Site1.Mvc5.Models
{
    public class RegisterForm
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}