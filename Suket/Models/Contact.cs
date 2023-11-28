using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suket.Models
{
    public class Contact
    {
        public int ContactId { get; set; }

        [Required]
        public string Message { get; set; }

        public string? Name { get; set; }

        [Required]
        public string? Email { get; set; }

        public DateTimeOffset Created { get; set; }

        public string? UserAccountId { get; set; }
        [ForeignKey("UserAccountId")]
        [ValidateNever]
        public virtual UserAccount? UserAccount { get; set; }
    }
}
