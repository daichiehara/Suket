namespace Suket.Models
{
    public class ChatDayViewModel
    {
        public DateTime Date { get; set; }
        public IEnumerable<MessageViewModel> Messages { get; set; }
    }
}
