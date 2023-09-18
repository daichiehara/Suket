using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Suket.Models;

namespace Suket.Areas.Identity.Pages.Account.Manage
{
    public class EarningsModel : PageModel
    {
        private readonly UserManager<UserAccount> _userManager;
        public bool? DetailsSubmitted { get; set; }
        public string? StripeAccountId { get; set; }

        public EarningsModel(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            DetailsSubmitted = user.DetailsSubmitted;
            StripeAccountId = user.StripeAccountId;

            return Page();
        }
    }
}
