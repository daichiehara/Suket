using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Suket.Models;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace Suket.Controllers
{
    public class StripeController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly AWSSecretsManagerService _awsSecretsManagerService;

        public StripeController(UserManager<UserAccount> userManager, AWSSecretsManagerService awsSecretsManagerService)
        {
            _userManager = userManager;
            _awsSecretsManagerService = awsSecretsManagerService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /*
        private static string GetStripeAPIKeyFromAzureKeyVault()
        {
            var keyVaultUrl = "https://stripetestapikey.vault.azure.net/";
            var secretName = "StripeTestAPIKey";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret stripeAPIKeySecret = client.GetSecret(secretName);

            return stripeAPIKeySecret.Value;
        }
        */

        /*
        private static async Task<string> GetStripeAPIKeyFromAWSSecretsManager()
        {
            string secretName = "MintSPORTS_secret";  // シークレットの名前を変更
            string region = "ap-northeast-1";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            // JSONからStripeAPIKeyを取得
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
            if (secrets != null && secrets.ContainsKey("StripeAPIKey"))
            {
                return secrets["StripeAPIKey"];
            }

            throw new Exception("StripeAPIKey not found in the secret.");
        }
        */

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> HasStripeAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var stripeAccountStatus = new
            {
                HasStripeAccountId = !string.IsNullOrWhiteSpace(user.StripeAccountId),
                DetailsSubmitted = user.DetailsSubmitted
            };
            return Json(stripeAccountStatus);
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateAccount()
        {
            //StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
            var stripeApiKey = await _awsSecretsManagerService.GetSecretAsync("MintSPORTS_secret");
            StripeConfiguration.ApiKey = stripeApiKey;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            string ProfileUrl = $"https://mintsports.net/Users/{user.UserName}";

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
                BusinessProfile = new AccountBusinessProfileOptions { Url = ProfileUrl },
            };
            var service = new AccountService();
            var account = service.Create(options);

            // Save the Stripe account ID to the user
            user.StripeAccountId = account.Id;
            //user.DetailsSubmitted = account.DetailsSubmitted;
            //user.ChargesEnabled= account.ChargesEnabled;
            await _userManager.UpdateAsync(user);

            // Create an account link
            return RedirectToAction("CreateAccountLink");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateAccountLink()
        {
            //StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
            var stripeApiKey = await _awsSecretsManagerService.GetSecretAsync("MintSPORTS_secret");
            StripeConfiguration.ApiKey = stripeApiKey;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var options = new AccountLinkCreateOptions
            {
                Account = user.StripeAccountId,
                RefreshUrl = "https://mintsports.net/Stripe/CreateAccountLink",
                ReturnUrl = "https://mintsports.net/Home/SuccessCreateStripeAccount",
                Type = "account_onboarding",
            };
            var service = new AccountLinkService();
            var accountLink = service.Create(options);

            // Redirect the user to the account link URL
            return Redirect(accountLink.Url);
        }

        /*
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateAccountDetails()
        {
            StripeConfiguration.ApiKey = GetStripeAPIKeyFromAzureKeyVault();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var service = new AccountService();
            var account = service.Get(user.StripeAccountId);

            // Update the user's account details
            user.DetailsSubmitted = account.DetailsSubmitted;
            user.ChargesEnabled = account.ChargesEnabled;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index", "Posts");
        }
        */
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Verify the webhook signature (for security)
            //test endpoint
            const string endpointSecret = "whsec_f8cd12c25d871ad7eb78bc7549def9b8faac91b44837832b2a8cc1403f4ce01b";
            //const string endpointSecret = "whsec_JHw9OGvxbHQxc0HyVxMpC1kmOkC9p0W5";
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Account;
                    // Retrieve the corresponding user in your database
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.StripeAccountId == account.Id);

                    if (user != null)
                    {
                        user.DetailsSubmitted = account.DetailsSubmitted;
                        user.ChargesEnabled = account.ChargesEnabled;
                        await _userManager.UpdateAsync(user);
                    }
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }


                // Return a response to Stripe to acknowledge receipt of the event
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize(Roles ="Admin")]
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
                //StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
                var stripeApiKey = await _awsSecretsManagerService.GetSecretAsync("MintSPORTS_secret");
                StripeConfiguration.ApiKey = stripeApiKey;
                var service = new AccountService();
                service.Delete(user.StripeAccountId);

                // Clear StripeAccountId for the user in your database
                user.StripeAccountId = null;
                user.DetailsSubmitted = false;
                user.ChargesEnabled = false;
                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StripeDashboard()
        {
            //StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
            var stripeApiKey = await _awsSecretsManagerService.GetSecretAsync("MintSPORTS_secret");
            StripeConfiguration.ApiKey = stripeApiKey;

            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.StripeAccountId))
            {
                // ユーザーが見つからない場合、またはStripeAccountIdが設定されていない場合の処理。
                // 例えば、エラーメッセージを表示する、または別のページにリダイレクトする等。
                return NotFound();
            }

            var service = new LoginLinkService();
            var loginLink = service.Create(user.StripeAccountId);

            return Redirect(loginLink.Url);
        }



        //const string endpointSecret = "whsec_f8cd12c25d871ad7eb78bc7549def9b8faac91b44837832b2a8cc1403f4ce01b";

        /*
        [HttpPost]
        public async Task<IActionResult> ProcessWebhookEvent()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var chargeId = paymentIntent.Charges.Data[0].Id;

                    // Use chargeId for your purposes.
                    // Store chargeId into your database for later usage in transfers.

                    handlePaymentIntentSucceeded(paymentIntent, chargeId);
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
        */
    }
}
