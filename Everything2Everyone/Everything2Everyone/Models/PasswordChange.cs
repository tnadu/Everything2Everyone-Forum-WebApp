using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class PasswordChange
    {
        public string UserID { get; set; }

        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password, ErrorMessage = "Provided password is invalid.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password, ErrorMessage = "Provided password is invalid.")]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
