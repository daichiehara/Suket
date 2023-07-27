using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Suket.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Required]
        [Display(Name = "マナーレベル")]
        [Range(1, 5)]
        public int MannerLevel { get; set; }


        [Display(Name = "スキルレベル")]
        [Range(1, 5)]
        //[Remote(action: "VerifySkillLevelRequirement", controller: "Reviews", AdditionalFields = nameof(ReviewerId) + "," + nameof(PostId) + "," + nameof(SkillLevel))]
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
