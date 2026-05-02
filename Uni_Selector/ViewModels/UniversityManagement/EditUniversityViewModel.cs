using System.ComponentModel.DataAnnotations;

namespace Uni_Selector.ViewModels.UniversityManagement
{
    public class EditUniversityViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters")]
        [Display(Name = "Name (English)")]
        public string NameEnglish { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters")]
        [Display(Name = "Name (Arabic)")]
        public string NameArabic { get; set; }

        [Required(ErrorMessage = "Province is required")]
        [StringLength(100)]
        [Display(Name = "Province")]
        public string Province { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Full address is required")]
        [StringLength(500)]
        [Display(Name = "Full Address")]
        public string FullAddress { get; set; }

        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }

        [StringLength(200)]
        [Display(Name = "Academic Accreditation")]
        public string? AcademicAccreditation { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Url(ErrorMessage = "Invalid website URL")]
        [StringLength(500)]
        [Display(Name = "Official Website")]
        public string? OfficialWebsite { get; set; }

        [Display(Name = "Logo")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }

        // Current file paths (for display)
        public string? CurrentLogoPath { get; set; }
        public string? CurrentImagePath { get; set; }

        // Permissions (for UI display)
        public bool CanEditBasicInfo { get; set; }
        public bool CanUploadImages { get; set; }
    }
}
