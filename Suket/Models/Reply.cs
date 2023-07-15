using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Suket.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }

        [Display(Name = "メッセージ")]
        [StringLength(200, ErrorMessage = "メッセージは200文字以内で入力してください。")]
        public string Message { get; set;}
        
        [Display(Name = "投稿日時")]
        public DateTimeOffset Created { get; set; }

        public string UserAccountId { get; set; }
        [ForeignKey("UserAccountId")]
        [ValidateNever]
        public virtual UserAccount UserAccount { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        [ValidateNever]
        public virtual Post Post { get; set; }
    }
}
