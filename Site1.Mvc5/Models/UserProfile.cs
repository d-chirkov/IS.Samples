using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Site1.Mvc5.Models
{
    public class UserProfile
    {
        public int UserProfileId { get; set; }
        public int IdSrvId { get; set; }
        public string Login { get; set; }
    }
}