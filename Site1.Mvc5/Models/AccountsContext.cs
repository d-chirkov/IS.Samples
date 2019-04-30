using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Site1.Mvc5.Models
{
    public class AccountsContext : DbContext
    {
        public AccountsContext() : base("AccountsDbContext")
        {
        }
        // Entities        
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}