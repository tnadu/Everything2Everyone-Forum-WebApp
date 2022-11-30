using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    // Clasa Users va mosteni clasa IdentityUser care deja contine:
    // -> UserID, RoleID, Password, Email
    public class Users: IdentityUser
    {

        [Required(ErrorMessage = "First name is required! Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "First name is required! Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "First name is required! Length must be between 5 and 30 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required! Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Last name is required! Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Last name is required! Length must be between 5 and 30 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Nickname is required! Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Nickname is required! Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Nickname is required! Length must be between 5 and 30 characters.")]
        public string NickName { get; set; }

        public DateTime JoinDate { get; set; }

    }
}
