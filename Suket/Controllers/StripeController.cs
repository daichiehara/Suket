using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Suket.Models;

namespace Suket.Controllers
{
    public class StripeController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        public StripeController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        private static string GetStripeAPIKeyFromAzureKeyVault()
        {
            var keyVaultUrl = "https://stripetestapikey.vault.azure.net/";
            var secretName = "StripeTestAPIKey";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret stripeAPIKeySecret = client.GetSecret(secretName);

            return stripeAPIKeySecret.Value;
        }

        [HttpGet]
        public async Task<IActionResult> HasStripeAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var hasStripeAccount = !string.IsNullOrWhiteSpace(user.StripeAccountId);
            return Json(new { hasStripeAccount = hasStripeAccount });
        }



        [HttpGet]
        public async Task<IActionResult> CreateAccount()
        {
            StripeConfiguration.ApiKey = GetStripeAPIKeyFromAzureKeyVault();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var options = new AccountCreateOptions
            {
                Type = "express",
                Country = "JP",
                Email = user.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true,
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true,
                    },
                },
                BusinessType = "individual",
                BusinessProfile = new AccountBusinessProfileOptions { Url = "https://example.com" },
            };
            var service = new AccountService();
            var account = service.Create(options);

            // Save the Stripe account ID to the user
            user.StripeAccountId = account.Id;
            await _userManager.UpdateAsync(user);

            // Create an account link
            return RedirectToAction("CreateAccountLink");
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccountLink()
        {
            StripeConfiguration.ApiKey = GetStripeAPIKeyFromAzureKeyVault();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var options = new AccountLinkCreateOptions
            {
                Account = user.StripeAccountId,
                RefreshUrl = "https://localhost:7144/Stripe/CreateAccountLink",
                ReturnUrl = "https://localhost:7144/Posts",
                Type = "account_onboarding",
            };
            var service = new AccountLinkService();
            var accountLink = service.Create(options);

            // Redirect the user to the account link URL
            return Redirect(accountLink.Url);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStripeAccount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(user.StripeAccountId))
            {
                return BadRequest("No Stripe account found for this user.");
            }

            try
            {
                StripeConfiguration.ApiKey = GetStripeAPIKeyFromAzureKeyVault();
                var service = new AccountService();
                service.Delete(user.StripeAccountId);

                // Clear StripeAccountId for the user in your database
                user.StripeAccountId = null;
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
