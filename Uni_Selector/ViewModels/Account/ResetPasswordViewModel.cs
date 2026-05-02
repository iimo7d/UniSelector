using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least 8 characters", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
