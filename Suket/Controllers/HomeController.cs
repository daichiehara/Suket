using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Suket.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Suket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<UserAccount> _signInManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<UserAccount> signInManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            /*
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Posts");
            }
            */
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            return View();
        }



        public IActionResult Privacy()
        {
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            return View();
        }

        public IActionResult Tokutei()
        {
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            return View();
        }

        public IActionResult Tos()
        {
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            return View();
        }

        public IActionResult Maintenance()
        {
            return View();
        }

        public IActionResult Guide()
        {
            ViewData["HideNavbar"] = true; // navbarを非表示にする
            return View();
        }

        public IActionResult SuccessCreateStripeAccount()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}