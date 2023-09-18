using Microsoft.AspNetCore.Mvc;

namespace Suket.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Success(int postId)
        {
            ViewData["PostId"] = postId;
            return View("Success", postId);  // "PaymentSuccess"は上記のHTMLページの名前として想定
        }
    }
}
