namespace Suket.Models
{
    public class MessageDto
    {
        public string Content { get; set; } // メッセージ内容
        public DateTimeOffset SentTime { get; set; } // メッセージ送信時刻
        public string UserId { get; set; } // メッセージ送信者のユーザーID
    }
}
