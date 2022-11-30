using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }
        [Key]
        public int ArticleID { get; set; }
        public string UserID { get; set; }

        [Required(ErrorMessage = "Content is required! Length must not exceed 500 characters.")]
        [MaxLength(500, ErrorMessage = "Content is required! Length must not exceed 500 characters.")]
        public string Content { get; set; }

        // To be able to -> JOIN with Article
        public Article Article { get; set; }
    }
}
