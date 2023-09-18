// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

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
    public class EmailModel : PageModel
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ISuketEmailSender _emailSender;

        public EmailModel(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            ISuketEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Remote(action:"IsEmailAvailable", controller: "Users", ErrorMessage = "このメールアドレスは既に登録されています。")]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(UserAccount user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Confirm your email",
                    $"以下のボタンを押すことでメールアドレスの確認が完了し、すべてのサービスが利用できるようになります。<br />" + 
                    $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='display: inline-block; padding: 10px 20px; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>メールアドレスを確認する</a><br /><br />MintSPORTSサポートチーム");

                StatusMessage = "メールアドレスを変更するための確認リンクを送信しました。 メールを確認してください。";
                return RedirectToPage();
            }

            StatusMessage = "あなたのメールアドレスは変更されていません。";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"以下のボタンを押すことでメールアドレスの確認が完了し、すべてのサービスが利用できるようになります。<br />" +
                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='display: inline-block; padding: 10px 20px; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>メールアドレスを確認する</a><br /><br />Mint SPORTSサポートチーム");

            StatusMessage = "確認メールが送信されました。 メールを確認してください。";
            return RedirectToPage();
        }
    }
}
