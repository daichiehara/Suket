using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace Suket.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> CreateChatRoom(string userId)
        {
            // ログインしているユーザーを取得
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // チャットルームが既に存在するか検索
            var existingChatRoom = _context.UserChatRoom
                .Where(ucr => (ucr.UserAccountId == currentUser.Id && ucr.ChatRoom.UserChatRooms.Any(x => x.UserAccountId == userId))
                    || (ucr.UserAccountId == userId && ucr.ChatRoom.UserChatRooms.Any(x => x.UserAccountId == currentUser.Id)))
                .Select(ucr => ucr.ChatRoom)
                .FirstOrDefault();

            string chatRoomId;
            if (existingChatRoom != null)
            {
                // 既存のチャットルームを使用
                chatRoomId = existingChatRoom.ChatRoomId;
            }
            else
            {
                // 新規チャットルームの作成
                var newChatRoom = new ChatRoom
                {
                    ChatRoomId = Guid.NewGuid().ToString(), // チャットルームIDとしてGUIDを使用
                    LastMessageTime = DateTimeOffset.UtcNow
                };
                _context.ChatRoom.Add(newChatRoom);
                await _context.SaveChangesAsync();

                // チャットルームとユーザーの関連付け
                _context.UserChatRoom.AddRange(
                    new UserChatRoom { UserAccountId = currentUser.Id, ChatRoomId = newChatRoom.ChatRoomId },
                    new UserChatRoom { UserAccountId = userId, ChatRoomId = newChatRoom.ChatRoomId }
                );
                await _context.SaveChangesAsync();

                chatRoomId = newChatRoom.ChatRoomId;
            }

            // チャット画面（View）に遷移
            return RedirectToAction("Chat", new { chatRoomId = chatRoomId });
        }

        [Authorize]
        public async Task<IActionResult> Chat(string chatRoomId)
        {
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            ViewData["Footer"] = true;
            
            var messages = await _context.Message
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentTime) // 最新のメッセージから順に並べる
                .Take(30) // 最新の30件のみを取得
                .Select(m => new MessageViewModel
                {
                    Content = m.Content,
                    SentTime = m.SentTime,
                    UserId = m.UserAccountId
                })
                .AsNoTracking() // トラッキング不要のためパフォーマンス向上
                .ToListAsync();

            messages.Reverse(); // 取得したメッセージを昇順に並べ替える

            var user = await _userManager.GetUserAsync(User);
            var chatRoom = await _context.UserChatRoom
                .Where(cr => cr.UserAccountId != user.Id)
                .Include(cr => cr.UserAccount) // ChatRoomとUserAccountsの間のリレーションを想定
                .FirstOrDefaultAsync(cr => cr.ChatRoomId == chatRoomId);

            ViewBag.ChatPartnerName = !string.IsNullOrEmpty(chatRoom?.UserAccount.NickName) ? chatRoom.UserAccount.NickName : chatRoom.UserAccount.UserName;
            ViewBag.CurrentUserId = user?.Id; // 現在のユーザーIDをViewBagに設定
            ViewBag.ChatRoomId = chatRoomId;
            ViewBag.ProfileImg = chatRoom.UserAccount.ProfilePictureUrl; // 仮定
            ViewBag.UserName = chatRoom.UserAccount.UserName;

            // チャットルームを閲覧するユーザーの未読メッセージカウントをリセット
            await MarkMessagesAsRead(chatRoomId, user.Id);
            return View(messages); // メッセージのリストをビューに渡す
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LoadMoreMessages(string chatRoomId, int pageNumber, int pageSize = 30)
        {
            var skipAmount = pageNumber * pageSize;

            var messages = await _context.Message
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderByDescending(m => m.SentTime)
                .Skip(skipAmount)
                .Take(pageSize)
                .Select(m => new MessageViewModel
                {
                    Content = m.Content,
                    SentTime = m.SentTime,
                    UserId = m.UserAccountId
                })
                .AsNoTracking()
                .ToListAsync();

            return Json(messages);
        }

        [Authorize]
        public async Task<IActionResult> ChatRooms()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // 現在のユーザーが属するチャットルームと関連データを取得
            var chatRooms = await _context.UserChatRoom
                .Where(ucr => ucr.UserAccountId == currentUser.Id)
                .Select(ucr => new
                {
                    ucr.ChatRoomId,
                    ucr.ChatRoom.LastMessageTime,
                    ChatPartnerName = _context.UserChatRoom
                        .Where(x => x.ChatRoomId == ucr.ChatRoomId && x.UserAccountId != currentUser.Id)
                        .Include(x => x.UserAccount)
                        .Select(x => x.UserAccount.UserName) // ここでチャット相手のユーザー名を選択
                        .FirstOrDefault(),
                    LastMessage = _context.Message
                        .Where(m => m.ChatRoomId == ucr.ChatRoomId)
                        .OrderByDescending(m => m.SentTime)
                        .Select(m => m.Content)
                        .FirstOrDefault(), // ここで最後のメッセージを取得
                    UserImg = _context.UserChatRoom
                        .Where(x => x.ChatRoomId == ucr.ChatRoomId && x.UserAccountId != currentUser.Id)
                        .Include(x => x.UserAccount)
                        .Select(x => x.UserAccount.ProfilePictureUrl) // ここでチャット相手のユーザー名を選択
                        .FirstOrDefault(),
                    UnreadMessagesCount = _context.UserChatRoom
                        .Where(m => m.ChatRoomId == ucr.ChatRoomId && m.UserAccountId == currentUser.Id) // 未読メッセージのカウント
                        .Select(x => x.UnreadMessagesCount)
                        .FirstOrDefault(),
                })
                .Where(c => c.LastMessage != null) // 最後のメッセージが存在するチャットルームのみを選択
                .ToListAsync();

            // 取得したデータを ChatRoomViewModel に変換
            var chatRoomViewModels = chatRooms.Select(c => new ChatRoomViewModel
            {
                ChatRoomId = c.ChatRoomId,
                LastMessageTime = c.LastMessageTime,
                ChatRoomName = c.ChatPartnerName,
                LastMessage = c.LastMessage,
                UserImg = c.UserImg,
                UnreadMessagesCount = c.UnreadMessagesCount
            }).ToList();

            return View(chatRoomViewModels);
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

        public IActionResult Index()
        {
            return View();
        }
    }
}
