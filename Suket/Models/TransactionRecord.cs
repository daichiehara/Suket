using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suket.Models
{
    public enum TransactionType
    {
        [Display(Name = "支払い")]
        Payment,    // 支払い
        [Display(Name = "送金")]
        Transfer,   // 送金
        [Display(Name = "返金")]
        Refund,     // 返金
        [Display(Name = "受け取り")]
        Receipt,    // 受け取り
        [Display(Name = "没収")]
        Lost        //没収
    }
    public class TransactionRecord
    {
        public int Id { get; set; }
        public string UserAccountId { get; set; }
        
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public int? PostId { get; set; }

        public string? PaymentIntentId { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        // 送金が完了したかどうかを示すフィールド
        public bool IsTransferred { get; set; } = false;

        [ForeignKey("UserAccountId")]
        public virtual UserAccount UserAccount { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}
