namespace Suket.Models
{
    public class ChatRoom
    {
        public string ChatRoomId { get; set; }
        public DateTimeOffset LastMessageTime { get; set; }

        public virtual ICollection<UserChatRoom> UserChatRooms { get; set; }
    }
}
