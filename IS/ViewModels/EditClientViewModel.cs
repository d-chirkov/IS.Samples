using System.ComponentModel.DataAnnotations;

namespace IS.ViewModels
{
    public class EditClientViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Secret { get; set; }

        public string Uri { get; set; }
    }
}