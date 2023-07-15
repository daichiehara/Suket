using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Suket.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Display(Name = "マナーレベル")]
        [Range(1, 5)]
        public int MannerLevel { get; set; }

        [Display(Name = "スキルレベル")]
        [Range(1, 5)]
        public int? SkillLevel { get; set; }

        [Display(Name = "レビューしたユーザー")]
        [ForeignKey("Reviewer")]
        public string ReviewerId { get; set; }
        public virtual UserAccount Reviewer { get; set; }

        [Display(Name = "レビューされたユーザー")]
        [ForeignKey("Reviewed")]
        public string ReviewedId { get; set; }
        public virtual UserAccount Reviewed { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}
