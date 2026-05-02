using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }

}
