using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Everything2Everyone.Models
{
    public class ArticleVersion
    {
        public int ArticleID { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int? CategoryID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VersionID { get; set; }

        [Required(ErrorMessage = "Title is required. Length must be between 5 and 50 characters.")]
        [MaxLength(50, ErrorMessage = "Title is required. Length must be between 5 and 50 characters.")]
        [MinLength(5, ErrorMessage = "Title is required. Length must be between 5 and 50 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Commit Title is required. Length must be between 5 and 20 characters.")]
        [MaxLength(20, ErrorMessage = "Commit Title is required. Length must be between 5 and 20 characters.")]
        [MinLength(5, ErrorMessage = "Commit Title is required. Length must be between 5 and 20 characters.")]
        public string CommitTitle { get; set; } = "CommitTitle";

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy - HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime CommitDate { get; set; }

        public virtual Article? Article { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<ChapterVersion>? Chapters { get; set; }
    }
}
