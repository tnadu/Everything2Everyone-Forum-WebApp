using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class ArticleVersion
    {
        [Key]
        public int ArticleID { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int? CategoryID { get; set; }

        [Key]
        public int VersionID { get; set; }

        // No validations required, since the only way
        // an article version can be inserted here is 
        // if it already passed the valdiation in the
        // Article Model
        public string Title { get; set; }

        public string CommitTitle { get; set; }

        public DateTime CommitDate { get; set; }

        public virtual Article? Article { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<ChapterVersion>? Chapters { get; set; }
    }
}
