using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everything2Everyone.Models
{
    public class ArticleBundle
    {
        public Article Article { get; set; }

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();

        [NotMapped]
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
