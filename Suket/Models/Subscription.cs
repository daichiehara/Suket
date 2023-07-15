using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suket.Models
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
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
