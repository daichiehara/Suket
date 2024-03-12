using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace Suket.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset SentTime { get; set; }
        public string UserAccountId { get; set; } // 送信者
        public UserAccount UserAccount { get; set; }
        public string ChatRoomId { get; set; } // 送信先チャットルーム
        public ChatRoom ChatRoom { get; set; }
    }
}
