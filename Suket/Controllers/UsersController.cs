// UsersController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace Suket.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<UserAccount> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // UsersController.cs
        public async Task<IActionResult> Profile(string UserName)
        {
            if (UserName == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == UserName);

            if (user == null)
            {
                return NotFound();
            }

            ViewData["HideNavbar"] = true; // navbarを非表示にする

            return View(user);
        }

        
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> IsUserNameInUse([Bind(Prefix = "Input.UserName")] string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user == null ? Json(true) : Json($"ユーザー名 {username} は既に存在しています。");
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> IsEmailInUse([Bind(Prefix = "Input.Email")] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? Json(true) : Json($"メールアドレス {email} は既に存在しています。");
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> IsEmailAvailable([Bind(Prefix = "Input.NewEmail")] string newEmail)
        {
            var user = await _userManager.FindByEmailAsync(newEmail);
            if (user == null)
                return Json(true);
            else
                return Json($"このメールアドレスは既に登録されています。");
        }



        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new UserAccount { UserName = model.UserName, Email = model.Email, Birthday = model.Birthday };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //await _userManager.AddToRoleAsync(user, "Member"); // Adjust based on your role system.
                    return RedirectToAction("Posts"); // Redirects to the Profile page after a successful registration.
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // 残高と取引履歴を表示するアクション
        [Authorize]
        public async Task<IActionResult> Balance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var balance = await _context.UserBalance.FirstOrDefaultAsync(ub => ub.Id == user.Id);
            var transactions = await _context.TransactionRecord
                .Include(t => t.Post) // Post データを事前に読み込む
                .Where(tr => tr.UserAccountId == user.Id)
                .OrderByDescending(tr => tr.TransactionDate) // TransactionDateで降順にソート
                .ToListAsync();

            var viewModel = new BalanceViewModel
            {
                Balance = balance,
                Transactions = transactions
            };

            return View(viewModel);
        }
    }

}