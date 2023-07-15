using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Suket.Models
{
    public class Adoption
    {
        public int AdoptionId { get; set; }
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
