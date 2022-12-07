using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class ArticleBundle
    {
        public Article Article { get; set; }

        public ICollection<Chapter> Chapters { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
