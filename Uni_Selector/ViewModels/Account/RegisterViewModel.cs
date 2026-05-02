using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.Account
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Name must be between 2 and 100 characters", MinimumLength = 2)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
