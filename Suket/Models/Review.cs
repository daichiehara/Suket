using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Suket.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Required]
        [Display(Name = "マナーレベル")]
        [Range(1, 5)]
        public int MannerLevel { get; set; }

        [Required]
        [Display(Name = "スキルレベル")]
        [Range(1, 5)]
        public int? SkillLevel { get; set; }

        [Required]
        [Display(Name = "レビューしたユーザー")]
        [ForeignKey("Reviewer")]
        public string ReviewerId { get; set; }
        [ValidateNever]
        public virtual UserAccount Reviewer { get; set; }

        [Required]
        [Display(Name = "レビューされたユーザー")]
        [ForeignKey("Reviewed")]
        public string ReviewedId { get; set; }
        [ValidateNever]
        public virtual UserAccount Reviewed { get; set; }

        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }
        [ValidateNever]
        public virtual Post Post { get; set; }
    }
}
