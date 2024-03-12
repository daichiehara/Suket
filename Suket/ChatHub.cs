using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Suket.Controllers;
using Suket.Data;
using Suket.Models;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace Suket
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly ISuketEmailSender _emailSender;

        public ChatHub( ApplicationDbContext context, UserManager<UserAccount> userManager, ISuketEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [Authorize]
        public async Task SendMessage(string message, string chatRoomId)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null)
            {
                // ユーザーが取得できない場合の処理
                return;
            }

            var chatRoomUsers = await _context.UserChatRoom
                .Where(cru => cru.ChatRoomId == chatRoomId && cru.UserAccountId != user.Id)
                .Include(cru => cru.UserAccount)  // UserAccount を含めて取得
                .ToListAsync();
            foreach (var chatRoomUser in chatRoomUsers)
            {
                chatRoomUser.UnreadMessagesCount += 1;
                _context.Update(chatRoomUser);

                var displayName = !string.IsNullOrWhiteSpace(user.NickName) ? user.NickName : user.UserName;  // NickName がなければ UserName を使用
                var emailAddress = chatRoomUser.UserAccount.Email;  // Email アドレスの取得
                var subject = string.Format(Resources.Notification.NewMessageSubject, displayName);
                var body = string.Format(Resources.Notification.NewMessageBody, displayName, message);
                // Send a notification to the poster
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    await _emailSender.SendEmailAsync(emailAddress, subject, body);
                }

            }

            await _context.SaveChangesAsync();

            // メッセージをデータベースに保存
            var msg = new Message
            {
                Content = message,
                SentTime = DateTimeOffset.UtcNow,
                UserAccountId = user.Id,
                ChatRoomId = chatRoomId
            };
            _context.Message.Add(msg);
            await _context.SaveChangesAsync();

            // 関連するチャットルームの LastMessageTime を更新
            var chatRoom = await _context.ChatRoom.FindAsync(chatRoomId);
            if (chatRoom != null)
            {
                chatRoom.LastMessageTime = msg.SentTime;
                _context.ChatRoom.Update(chatRoom);
                await _context.SaveChangesAsync();
            }


            // DTOを使用してメッセージをブロードキャスト
            var messageDto = new MessageDto
            {
                Content = message,
                SentTime = msg.SentTime,
                UserId = user.Id
            };

            await Clients.All.SendAsync("ReceiveMessage", messageDto);
        }

        [Authorize]
        public async Task MarkMessagesAsRead(string chatRoomId, string userId)
        {
            var chatRoomUser = await _context.UserChatRoom
                                    .FirstOrDefaultAsync(cru => cru.ChatRoomId == chatRoomId && cru.UserAccountId == userId);
            if (chatRoomUser != null)
            {
                chatRoomUser.UnreadMessagesCount = 0;
                _context.Update(chatRoomUser);
                await _context.SaveChangesAsync();
            }
        }

    }
}
