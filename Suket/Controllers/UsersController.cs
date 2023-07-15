// UsersController.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Suket.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace Suket.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        public UsersController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
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

            return View(user);
        }

        /*
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> IsUserNameInUse(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user == null ? Json(true) : Json($"Username {username} is already in use.");
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? Json(true) : Json($"Email {email} is already in use.");
        }

        public IActionResult Register()
        {
            return View();
        }
        */
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

        
    }

}