namespace Suket.Models
{
    public class MessageViewModel
    {
        public string Content { get; set; } // メッセージの内容
        public DateTimeOffset SentTime { get; set; } // メッセージが送信された時刻

        public string UserId { get; set; } // メッセージ送信者のユーザーID

        // 必要に応じて他のプロパティも追加できます。例えば、ユーザーのプロファイル画像のURLなど。
    }
}
