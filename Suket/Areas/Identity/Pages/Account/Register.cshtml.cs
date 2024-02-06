// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Suket.Data;
using Suket.Models;

namespace Suket.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUserStore<UserAccount> _userStore;
        private readonly IUserEmailStore<UserAccount> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ISuketEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<UserAccount> userManager,
            IUserStore<UserAccount> userStore,
            SignInManager<UserAccount> signInManager,
            ILogger<RegisterModel> logger,
            ISuketEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<UserAccount>)GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [DataType(DataType.Text)]
            [Remote(action: "IsUserNameInUse", controller: "Users")]
            [Display(Name = "ユーザー名")]
            [RegularExpression(@"^[a-zA-Z0-9]+[a-zA-Z0-9-_.]*[a-zA-Z0-9]+$", ErrorMessage = "ユーザー名はアルファベット、数字、ハイフン、アンダースコア、ドットのみ使用できます。ハイフン、アンダースコア、ドットは文字列の最初と最後には使用できません。")]
            public string UserName { get; set; }

            
            

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Remote(action: "IsEmailInUse", controller: "Users")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            
            [Required]
            [Display(Name ="生年月日")]
            public DateOnly Birthday { get; set; }
            
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync((UserAccount)user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync((UserAccount)user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync((UserAccount)user, Input.Password);

                if (result.Succeeded)
                {
                    // UserBalanceの作成と保存
                    var userBalance = new UserBalance
                    {
                        Id = user.Id, // UserAccountのIDを使用
                        Balance = 0, // 初期バランスを0に設定
                        LastUpdated = DateTimeOffset.UtcNow // 現在の日時を設定
                    };
                    // DbContextにUserBalanceを追加して保存
                    _context.UserBalance.Add(userBalance);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User created a new account with password.");

                    // Add user to the 'User' role
                    await _userManager.AddToRoleAsync((UserAccount)user, "User");
                    var userId = await _userManager.GetUserIdAsync((UserAccount)user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync((UserAccount)user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "メールアドレスの確認",
                        $"サインアップありがとうございます。 <br/> <br/>以下のボタンを押すことでメールアドレスの確認が完了し、サービスが利用できるようになります。<br />" +
                        $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='display: inline-block; padding: 10px 20px; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>メールアドレスを確認する</a><br/>Mint SPORTSサポートチーム");

                    
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync((UserAccount)user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                
                var users = new UserAccount
                {
                    UserName = Input.UserName,
                    Birthday = Input.Birthday
                };



                //return Activator.CreateInstance<UserAccount>();
                return users;
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<UserAccount> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<UserAccount>)_userStore;
        }
    }
}
