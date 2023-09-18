using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suket.Models
{
    public class PaymentRecord
    {
        public int Id { get; set; }
        public string PaymentIntentId { get; set; }
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        [ValidateNever]
        public virtual Post Post { get; set; }
        public string UserAccountId { get; set; }
        // 必要に応じて他の情報、例：TransferId、支払いのステータス、日付、エラーメッセージ等
        [ForeignKey("UserAccountId")]
        [ValidateNever]
        public virtual UserAccount UserAccount { get; set; }

        public bool Refunded { get; set; }
    }
}
