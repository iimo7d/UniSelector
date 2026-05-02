using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.UniversityAdmin
{
    public class CreateUniversityViewModel
    {
        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters")]
        [Display(Name = "English Name")]
        public string NameEnglish { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters")]
        [Display(Name = "Arabic Name")]
        public string NameArabic { get; set; } = string.Empty;

        [Required(ErrorMessage = "University type is required")]
        [Display(Name = "University Type")]
        public UniversityType Type { get; set; }

        [Required(ErrorMessage = "Province is required")]
        [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Full Address")]
        public string FullAddress { get; set; } = string.Empty;

        [Display(Name = "Latitude")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Display(Name = "Longitude")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        [StringLength(200, ErrorMessage = "Academic accreditation cannot exceed 200 characters")]
        [Display(Name = "Academic Accreditation")]
        public string? AcademicAccreditation { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Website URL cannot exceed 500 characters")]
        [Display(Name = "Official Website")]
        public string? OfficialWebsite { get; set; }

        [Required(ErrorMessage = "Commission mode is required")]
        [Display(Name = "Commission Mode")]
        public CommissionMode CommissionMode { get; set; }

        [Display(Name = "University Logo")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }
    }
}
