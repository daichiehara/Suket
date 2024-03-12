using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace Suket.Models
{
    public class UserChatRoom
    {
        public string UserAccountId { get; set; }
        public UserAccount UserAccount { get; set; }
        public string ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public int UnreadMessagesCount { get; set; }
    }
}
