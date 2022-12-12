using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class User: IdentityUser
    {

        [Required(ErrorMessage = "First name is required. Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "First name is required. Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "First name is required. Length must be between 5 and 30 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required. Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Last name is required. Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Last name is required. Length must be between 5 and 30 characters.")]
        public string LastName { get; set; }

        public bool ShowPublicIdentity { get; set; }

        [Required(ErrorMessage = "Nickname is required. Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Nickname is required. Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Nickname is required. Length must be between 5 and 30 characters.")]
        public string NickName { get; set; }

        public DateTime JoinDate { get; set; }

        public virtual ICollection<Role>? Roles { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Article>? Articles { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> FetchedRoles { get; set; }

        [NotMapped]
        public string? NewRoleID { get; set; }
    }
}
