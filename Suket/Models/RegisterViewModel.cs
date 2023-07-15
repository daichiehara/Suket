using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Suket.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Remote(action: "IsUserNameInUse", controller: "Users")]
        [Display(Name = "ユーザー名")]
        [RegularExpression(@"^[a-zA-Z0-9]+[a-zA-Z0-9-_.]*[a-zA-Z0-9]+$", ErrorMessage = "ユーザー名はアルファベット、数字、ハイフン、アンダースコア、ドットのみ使用できます。ハイフン、アンダースコア、ドットは文字列の最初と最後には使用できません。")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Users")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "生年月日")]
        public DateOnly Birthday { get; set; }
    }
}
