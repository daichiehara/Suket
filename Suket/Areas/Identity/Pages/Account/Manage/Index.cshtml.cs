// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Suket.Models;

namespace Suket.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;

        public IndexModel(
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

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
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [DataType(DataType.Text)]
            [Display(Name = "ユーザー名")]
            [RegularExpression(@"^[a-zA-Z0-9]+[a-zA-Z0-9-_.]*[a-zA-Z0-9]+$", ErrorMessage = "ユーザー名はアルファベット、数字、ハイフン、アンダースコア、ドットのみ使用できます。ハイフン、アンダースコア、ドットは文字列の最初と最後には使用できません。")]
            public string UserName { get; set; }

            [StringLength(12, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name ="表示名")]
            public string NickName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "プロフィール文")]
            public string Profile { get; set; }

            [Display(Name = "性別")]
            public Sex SelectSex { get; set; }

            [Display(Name = "生年月日")]
            public DateOnly Birthday { get; set; }
        }

        private async Task LoadAsync(UserAccount user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var nickName = user.NickName;
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var selectSex = user.SelectSex;  // ユーザーの現在の性別を取得
            var profile = user.Profile;  // ユーザーの現在のプロフィールを取得
            
            // ユーザーの現在の誕生日を取得、もしユーザーの誕生日がnullならデフォルト値を2001年1月1日に設定
            var birthday = user.Birthday != default ? user.Birthday : new DateOnly(2001, 1, 1);

            Username = userName;

            Input = new InputModel
            {
                UserName= userName,
                NickName= nickName,
                PhoneNumber = phoneNumber,
                SelectSex = selectSex,  // 取得した性別をInputModelに設定
                Profile = profile,  // 取得したプロフィールをInputModelに設定
                Birthday = birthday
            };
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

        public async Task<IActionResult> OnPostAsync()
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

            var userName = await _userManager.GetUserNameAsync(user);
            if (Input.UserName != userName)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "ユーザー名 を設定しようとしたときに予期しないエラーが発生しました";
                    return RedirectToPage();
                }
            }
            var nickname = user.NickName;
            if (Input.NickName != nickname)
            {
                user.NickName = Input.NickName;
                await _userManager.UpdateAsync(user);
            }
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            var selectsex = user.SelectSex;
            if (Input.SelectSex != selectsex)
            {
                user.SelectSex = Input.SelectSex;
                await _userManager.UpdateAsync(user);
            }
            var profile = user.Profile;
            if (Input.Profile != profile)
            {
                user.Profile = Input.Profile;
                await _userManager.UpdateAsync(user);
            }
            var birthday = user.Birthday;
            if (Input.Birthday != birthday)
            {
                user.Birthday = Input.Birthday;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
