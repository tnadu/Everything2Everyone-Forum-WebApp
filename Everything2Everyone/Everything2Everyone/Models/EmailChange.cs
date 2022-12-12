using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class EmailChange
    {
        public string UserID { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "E-mail is required.")]
        [EmailAddress(ErrorMessage = "Provided e-mail is invalid.")]
        public string NewEmail { get; set; }
    }
}
