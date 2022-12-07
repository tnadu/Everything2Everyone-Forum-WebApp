using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class ArticleVersionBundle
    {
        public ArticleVersion Article { get; set; }

        public ICollection<ChapterVersion> Chapters { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
