using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Suket.Models
{
    public enum Sex
    {
        [Display(Name = "男性")]
        man = 1,
        [Display(Name = "女性")]
        woman = 2,
        [Display(Name = "その他")]
        Q = 3,
        [Display(Name = "回答なし")]
        sexnull = 0
    }
    public class UserAccount : IdentityUser
    {
        
        [Display(Name = "表示名")]
        // 入力をアルファベットと数字のみに限定する正規表現
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "表示名にはアルファベットと数字のみを使用できます。")]
        public string? NickName { get; set; }

        [Required]
        [Display(Name = "生年月日")]
        public DateOnly Birthday { get; set; }

        [Display(Name = "プロフィール文")]
        [StringLength(500, ErrorMessage = "プロフィール文は500文字以内で入力してください。")]
        public string? Profile { get; set; }

        [Display(Name = "性別")]
        public Sex SelectSex { get; set; }

        [Display(Name = "Icon")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "StripeId")]
        public string? StripeAccountId { get; set; }

        public bool DetailsSubmitted { get; set; }

        public bool ChargesEnabled { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<Adoption> Adoptions { get; set; }
        public virtual ICollection<Confirm> Confirms { get; set; }
        public virtual ICollection<Reply> Replys { get; set; }
        public virtual ICollection<RollCall> RollCalls { get; set; }
        public ICollection<Review> ReviewsWritten { get; set; }
        public ICollection<Review> ReviewsReceived { get; set; }
        public virtual ICollection<PaymentRecord> PaymentRecords { get; set; }
    }
}
