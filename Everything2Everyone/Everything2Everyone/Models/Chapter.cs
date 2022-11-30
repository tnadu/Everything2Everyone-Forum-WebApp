using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class Chapter
    {
        [Key]
        public string ChapterID { get; set; }
        [Key]
        public int ArticleID { get; set; }

        [Required(ErrorMessage = "Title is required! Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Title is required! Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Title is required! Length must be between 5 and 30 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Title is required! Length must not exceed 8000 characters.")]
        [MaxLength(8000, ErrorMessage = "Title is required! Length must not exceed 8000 characters.")]
        public string ContentUnparsed { get; set; }
        public string ContentParsed { get; set; }
    }
}
