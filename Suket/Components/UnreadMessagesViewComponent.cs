using Google.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;

namespace Suket.Components
{
    public class UnreadMessagesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public UnreadMessagesViewComponent(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id; // または適切な方法でユーザーIDを取得
            var unreadCount = await _context.UserChatRoom
                .Where(x => x.UserAccountId == userId)
                .SumAsync(x => x.UnreadMessagesCount); // 未読メッセージ数を集計

            return View(unreadCount); // 未読メッセージ数をビューに渡す
        }
    }
}
