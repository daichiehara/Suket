using Microsoft.AspNetCore.Mvc;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Success(int postId)
        {
            ViewData["PostId"] = postId;
            var post = await _context.Post.FindAsync(postId);
            if (post == null)
            {
                // 投稿が見つからない場合の処理。エラーページにリダイレクトするなど。
                return NotFound();
            }

            ViewData["PostId"] = postId;
            string baseUrl;
            if (post.PaymentType == PaymentType.RewardToParticipant)
            {
                baseUrl = $"https://mintsports.net/Posts/Subscriber/{postId}";
            }
            else
            {
                baseUrl = "https://mintsports.net/Posts/MyDashboard";
            }
            ViewData["BaseUrl"] = baseUrl;
            return View("Success", postId);  // "PaymentSuccess"は上記のHTMLページの名前として想定
        }
    }
}
