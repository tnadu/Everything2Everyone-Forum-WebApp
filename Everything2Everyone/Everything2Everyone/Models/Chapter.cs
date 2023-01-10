using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class Chapter
    {
        public int ChapterID { get; set; }

        public int ArticleID { get; set; }

        [Required(ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        [MinLength(5, ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        [MaxLength(30, ErrorMessage = "Title is required. Length must be between 5 and 30 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required. Length must be non-null and must not exceed 8000 characters.")]
        [MinLength(1, ErrorMessage = "Content is required. Length must be non-null and must not exceed 8000 characters.")]
        [MaxLength(8000, ErrorMessage = "Content is required. Length must be non-null and must not exceed 8000 characters.")]
        public string Content { get; set; }

        public virtual Article? Article { get; set; }
    }
}
