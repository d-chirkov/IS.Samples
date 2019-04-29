using System.ComponentModel.DataAnnotations;

namespace IS.ViewModels
{
    public class NewClientViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Secret { get; set; }

        public string Uri { get; set; }
    }
}