using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Site1.Mvc5.Models
{
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; set; }
        public string IdSrvId { get; set; }
        public string Login { get; set; }
    }
}