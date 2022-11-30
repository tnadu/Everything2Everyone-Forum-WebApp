using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        public string Title { get; set; }

    }
}
