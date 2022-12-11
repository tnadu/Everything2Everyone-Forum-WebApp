using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        public int ArticleID { get; set; }

        public string UserID { get; set; }

        [Required(ErrorMessage = "Content is required. Length must be non-null and must not exceed 500 characters.")]
        [MaxLength(500, ErrorMessage = "Content is required. Length must be non-null and must not exceed 500 characters.")]
        [MinLength(1, ErrorMessage = "Content is required. Length must be non-null and must not exceed 500 characters.")]
        public string Content { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime DateEdited { get; set; }

        public virtual Article? Article { get; set; }

        public virtual User? User { get; set; }
    }
}
