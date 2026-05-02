using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;
using Uni_Selector.ViewModels.AdminCommission;

namespace Uni_Selector.ViewModels.UserManagement
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Student-specific fields
        public int? StudentId { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Range(0, 100, ErrorMessage = "GPA must be between 0 and 100")]
        [Display(Name = "GPA")]
        public double? GPA { get; set; }

        [StringLength(100)]
        [Display(Name = "Province")]
        public string? Province { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "Path Type")]
        public PathType? Path { get; set; }

        [Display(Name = "Academic Track")]
        public AcademicTrack? AcademicTrack { get; set; }

        [Display(Name = "Vocational Branch")]
        public VocationalBranch? VocationalBranch { get; set; }

        [Display(Name = "Registration Budget")]
        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal? RegistrationBudget { get; set; }

        [Display(Name = "Profile Completed")]
        public bool? ProfileCompleted { get; set; }

        // University Representative specific fields
        public int? UniversityRepresentativeId { get; set; }

        [Display(Name = "University")]
        public int? UniversityId { get; set; }

        [StringLength(100)]
        [Display(Name = "Position/Title")]
        public string? Position { get; set; }

        // Available options for dropdowns
        public List<string> AvailableRoles { get; set; } = new();
        public List<UniversityOption> AvailableUniversities { get; set; } = new();

        public bool ProfileCompletedValue
        {
            get => ProfileCompleted ?? false;
            set => ProfileCompleted = value;
        }

    }
}
