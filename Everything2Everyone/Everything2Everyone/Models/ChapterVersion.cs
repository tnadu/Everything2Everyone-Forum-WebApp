using System.ComponentModel.DataAnnotations;

namespace Everything2Everyone.Models
{
    public class ChapterVersion
    {
        [Key]
        public string ChapterID { get; set; }
        [Key]
        public int ArticleID { get; set; }
        [Key]
        public int VersionID { get; set; }

        // Not setting constraints for this columns because
        // The table will be populated only when a change occurs in ChapterVersion
        public string Title { get; set; }
        public string ContentUnparsed { get; set; }
        public string ContentParsed { get; set; }
    }
}
