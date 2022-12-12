using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Everything2Everyone.Models
{
    public class Role : IdentityRole
    {
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
