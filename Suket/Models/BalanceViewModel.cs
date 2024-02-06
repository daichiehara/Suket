namespace Suket.Models
{
    public class BalanceViewModel
    {
        public UserBalance Balance { get; set; }
        public List<TransactionRecord> Transactions { get; set; }
    }
}
