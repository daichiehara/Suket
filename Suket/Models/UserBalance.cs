using System.ComponentModel.DataAnnotations.Schema;

namespace Suket.Models
{
    public class UserBalance
    {
        public string Id { get; set; }
        public decimal Balance { get; set; }
        public DateTimeOffset LastUpdated { get; set; }

        public virtual UserAccount UserAccount { get; set; }
    }
}
