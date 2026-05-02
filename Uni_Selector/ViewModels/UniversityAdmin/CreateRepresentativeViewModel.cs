using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class CreateRepresentativeViewModel
    {
        [Required]
        public int UniversityId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        [Display(Name = "Position")]
        public string? Position { get; set; }

        [Display(Name = "Can Manage Programs")]
        public bool CanManagePrograms { get; set; } = true;

        [Display(Name = "Can Manage Fees")]
        public bool CanManageFees { get; set; } = true;

        [Display(Name = "Can View Applications")]
        public bool CanViewApplications { get; set; } = true;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
