namespace Suket.Models
{
    public class ChatRoomViewModel
    {
        public string ChatRoomId { get; set; }
        public DateTimeOffset LastMessageTime { get; set; }
        public string ChatRoomName { get; set;}
        public string LastMessage { get; set;}
        public string UserImg { get; set;}
        public int UnreadMessagesCount { get; set; }
    }
}
